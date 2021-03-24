using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Xml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Vivian.CodeAnalysis.Binding;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;
using Vivian.CodeAnalysis.Text;

namespace Vivian.CodeAnalysis.Emit
{
    internal sealed class ILEmitter
    {
        private const TypeAttributes _classAttributes = TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;

        private readonly DiagnosticBag _diagnostics = new();
        private readonly Dictionary<SourceText, Document> _documents = new();
        private readonly Dictionary<ClassSymbol, TypeDefinition> _structs = new();
        private readonly Dictionary<FunctionSymbol, MethodDefinition> _methods = new();
        private readonly Dictionary<VariableSymbol, VariableDefinition> _locals = new();
        private readonly Dictionary<BoundLabel, int> _labels = new();
        private readonly List<(int InstructionIndex, BoundLabel Target)> _fixups = new();
        
        private readonly Dictionary<TypeSymbol, TypeReference> _knownTypes;

        private readonly MethodReference _consoleReadLineReference;
        private readonly MethodReference _consoleReadKeyReference;
        
        private readonly MethodReference _consoleWriteLineReference;
        private readonly MethodReference _consoleWriteReference;
        
        private readonly MethodReference _readAllTextReference;
        private readonly MethodReference _writeAllTextReference;

        private readonly MethodReference _objectCtor;
        private readonly MethodReference _objectEqualsReference;
        private readonly MethodReference _stringConcat2Reference;
        private readonly MethodReference _stringConcat3Reference;
        private readonly MethodReference _stringConcat4Reference;
        private readonly MethodReference _stringConcatArrayReference;
        private readonly MethodReference _convertToBooleanReference;
        private readonly MethodReference _convertToInt8Reference;
        private readonly MethodReference _convertToInt16Reference;
        private readonly MethodReference _convertToInt32Reference;
        private readonly MethodReference _convertToInt64Reference;
        private readonly MethodReference _convertToUInt8Reference;
        private readonly MethodReference _convertToUInt16Reference;
        private readonly MethodReference _convertToUInt32Reference;
        private readonly MethodReference _convertToUInt64Reference;
        private readonly MethodReference _convertToFloat32Reference;
        private readonly MethodReference _convertToFloat64Reference;
        private readonly MethodReference _convertToFloat128Reference;
        private readonly MethodReference _convertToCharReference;
        private readonly MethodReference _convertToStringReference;
        private readonly MethodReference _debuggableAttributeCtorReference;
        private readonly TypeReference _randomReference;
        private readonly MethodReference _randomCtorReference;
        private readonly MethodReference _randomNextReference;
        private readonly AssemblyDefinition _assemblyDefinition;
        
        private readonly TypeDefinition _typeDefinition;
        private FieldDefinition? _randomFieldDefinition;

        // TODO: This constructor does too much. Resolution should be factored out.
        private ILEmitter(string moduleName, string[] references)
        {
            var assemblies = new List<AssemblyDefinition>();

            foreach (var reference in references)
            {
                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly(reference);
                    assemblies.Add(assembly);
                }
                catch (BadImageFormatException)
                {
                    _diagnostics.ReportInvalidReference(reference);
                }
            }

            var builtInTypes = new List<(TypeSymbol Type, string MetadataName)>()
            {
                (TypeSymbol.Object, "System.Object"),
                (TypeSymbol.Bool, "System.Boolean"),
                (TypeSymbol.Int8, "System.SByte"),
                (TypeSymbol.Int16, "System.Int16"),
                (TypeSymbol.Int32, "System.Int32"),
                (TypeSymbol.Int64, "System.Int64"),
                (TypeSymbol.UInt8, "System.Byte"),
                (TypeSymbol.UInt16, "System.UInt16"),
                (TypeSymbol.UInt32, "System.UInt32"),
                (TypeSymbol.UInt64, "System.UInt64"),
                (TypeSymbol.Float32, "System.Single"),
                (TypeSymbol.Float64, "System.Double"),
                (TypeSymbol.Decimal, "System.Decimal"),
                (TypeSymbol.Char, "System.Char"),
                (TypeSymbol.String, "System.String"),
                (TypeSymbol.Void, "System.Void"),
            };

            var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
            _assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);
            _knownTypes = new Dictionary<TypeSymbol, TypeReference>();

            foreach (var (typeSymbol, metadataName) in builtInTypes)
            {
                var typeReference = ResolveType(typeSymbol.Name, metadataName);
                _knownTypes.Add(typeSymbol, typeReference);
            }

            TypeReference ResolveType(string? ev2Name, string metadataName)
            {
                var foundTypes = assemblies.SelectMany(a => a.Modules)
                                           .SelectMany(m => m.Types)
                                           .Where(t => t.FullName == metadataName)
                                           .ToArray();
                if (foundTypes.Length == 1)
                {
                    return _assemblyDefinition.MainModule.ImportReference(foundTypes[0]);
                }
                else if (foundTypes.Length == 0)
                {
                    _diagnostics.ReportRequiredTypeNotFound(ev2Name, metadataName);
                }
                else
                {
                    _diagnostics.ReportRequiredTypeAmbiguous(ev2Name, metadataName, foundTypes);
                }

                return null!;
            }

            MethodReference ResolveMethod(string typeName, string methodName, string[] parameterTypeNames)
            {
                var foundTypes = assemblies.SelectMany(a => a.Modules)
                                           .SelectMany(m => m.Types)
                                           .Where(t => t.FullName == typeName)
                                           .ToArray();

                if (foundTypes.Length == 1)
                {
                    var foundType = foundTypes[0];
                    var methods = foundType.Methods.Where(m => m.Name == methodName);

                    foreach (var method in methods)
                    {
                        if (method.Parameters.Count != parameterTypeNames.Length)
                        {
                            continue;
                        }

                        var allParametersMatch = true;

                        for (var i = 0; i < parameterTypeNames.Length; i++)
                        {
                            if (method.Parameters[i].ParameterType.FullName != parameterTypeNames[i])
                            {
                                allParametersMatch = false;
                                break;
                            }
                        }

                        if (!allParametersMatch)
                        {
                            continue;
                        }

                        return _assemblyDefinition.MainModule.ImportReference(method);
                    }

                    _diagnostics.ReportRequiredMethodNotFound(typeName, methodName, parameterTypeNames);
                    return null!;
                }
                else if (foundTypes.Length == 0)
                {
                    _diagnostics.ReportRequiredTypeNotFound(null, typeName);
                }
                else
                {
                    _diagnostics.ReportRequiredTypeAmbiguous(null, typeName, foundTypes);
                }

                return null!;
            }

            _readAllTextReference = ResolveMethod("System.IO.File", "ReadAllText", new [] { "System.String" });
            _writeAllTextReference = ResolveMethod("System.IO.File", "WriteAllText", new [] { "System.String", "System.String" });

            _objectCtor = ResolveMethod("System.Object", ".ctor", Array.Empty<string>());
            _objectEqualsReference = ResolveMethod("System.Object", "Equals", new [] { "System.Object", "System.Object" });
            
            
            _consoleReadLineReference = ResolveMethod("System.Console", "ReadLine", Array.Empty<string>());
            _consoleReadKeyReference = ResolveMethod("System.Console", "ReadKey", Array.Empty<string>());
            
            _consoleWriteLineReference = ResolveMethod("System.Console", "WriteLine", new [] { "System.Object" });
            _consoleWriteReference = ResolveMethod("System.Console", "Write", new[] { "System.Object" });
            
            _stringConcat2Reference = ResolveMethod("System.String", "Concat", new [] { "System.String", "System.String" });
            _stringConcat3Reference = ResolveMethod("System.String", "Concat", new [] { "System.String", "System.String", "System.String" });
            _stringConcat4Reference = ResolveMethod("System.String", "Concat", new [] { "System.String", "System.String", "System.String", "System.String" });
            _stringConcatArrayReference = ResolveMethod("System.String", "Concat", new [] { "System.String[]" });

            _convertToBooleanReference = ResolveMethod("System.Convert", "ToBoolean", new [] { "System.Object" });
            _convertToInt8Reference = ResolveMethod("System.Convert", "ToByte", new[] { "System.Object" });
            _convertToInt16Reference = ResolveMethod("System.Convert", "ToInt16", new [] { "System.Object" });
            _convertToInt32Reference = ResolveMethod("System.Convert", "ToInt32", new [] { "System.Object" });
            _convertToInt64Reference = ResolveMethod("System.Convert", "ToInt64", new [] { "System.Object" });
            _convertToUInt8Reference = ResolveMethod("System.Convert", "ToSByte", new[] { "System.Object" });
            _convertToUInt16Reference = ResolveMethod("System.Convert", "ToUInt16", new [] { "System.Object" });
            _convertToUInt32Reference = ResolveMethod("System.Convert", "ToUInt32", new [] { "System.Object" });
            _convertToUInt64Reference = ResolveMethod("System.Convert", "ToUInt64", new [] { "System.Object" });

            _convertToFloat32Reference = ResolveMethod("System.Convert", "ToSingle", new [] { "System.Object" });
            _convertToFloat64Reference = ResolveMethod("System.Convert", "ToDouble", new [] { "System.Object" });
            _convertToFloat128Reference = ResolveMethod("System.Convert", "ToDecimal", new [] { "System.Object" });

            _convertToCharReference = ResolveMethod("System.Convert", "ToChar", new [] { "System.Object" });
            _convertToStringReference = ResolveMethod("System.Convert", "ToString", new [] { "System.Object" });

            _randomReference = ResolveType(null, "System.Random");
            _randomCtorReference = ResolveMethod("System.Random", ".ctor", Array.Empty<string>());
            _randomNextReference = ResolveMethod("System.Random", "Next", new [] { "System.Int32" });
            _debuggableAttributeCtorReference = ResolveMethod("System.Diagnostics.DebuggableAttribute", ".ctor", new [] { "System.Boolean", "System.Boolean" });

            var objectType = _knownTypes[TypeSymbol.Object];

            if (objectType != null!)
            {
                _typeDefinition = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, objectType);
                _assemblyDefinition.MainModule.Types.Add(_typeDefinition);
            }
            else
            {
                _typeDefinition = null!;
            }
        }

        public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
        {
            if (program.Diagnostics.HasErrors())
            {
                return program.Diagnostics;
            }

            var emitter = new ILEmitter(moduleName, references);
            return emitter.Emit(program, outputPath);
        }

        public ImmutableArray<Diagnostic> Emit(BoundProgram program, string outputPath)
        {
            if (_diagnostics.Any())
            {
                return _diagnostics.ToImmutableArray();
            }

            foreach (var structWithBody in program.Structs)
            {
                EmitStructDeclaration(structWithBody.Key);
            }

            foreach (var (@struct, body) in program.Structs)
            {
                EmitStructBody(@struct, body);
            }

            foreach (var functionWithBody in program.Functions)
            {
                EmitFunctionDeclaration(functionWithBody.Key);
            }

            foreach (var (function, body) in program.Functions)
            {
                EmitFunctionBody(function, body);
            }

            if (program.MainFunction != null)
            {
                _assemblyDefinition.EntryPoint = _methods[program.MainFunction];
            }

            // TODO: should not emit this attribute unless we produce a debug build
            var debuggableAttribute = new CustomAttribute(_debuggableAttributeCtorReference);
            debuggableAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_knownTypes[TypeSymbol.Bool], true));
            debuggableAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_knownTypes[TypeSymbol.Bool], true));
            _assemblyDefinition.CustomAttributes.Add(debuggableAttribute);

            // TODO: should not be computing paths in here.
            var symbolsPath = Path.ChangeExtension(outputPath, ".pdb");

            // TODO: should support not emitting symbols
            using (var outputStream = File.Create(outputPath))
            using (var symbolsStream = File.Create(symbolsPath))
            {
                var writerParameters = new WriterParameters
                {
                    WriteSymbols = true,
                    SymbolStream = symbolsStream,
                    SymbolWriterProvider = new PortablePdbWriterProvider()
                };
                _assemblyDefinition.Write(outputStream, writerParameters);
            }

            return _diagnostics.ToImmutableArray();
        }

        private void EmitStructDeclaration(ClassSymbol key)
        {
            // Classes are actually implemented as classes to align more closely with the C-style understanding of structs.
            // They are reference types rather than the .NET value types.
            var classType = new TypeDefinition("", key.Name, _classAttributes, _knownTypes[TypeSymbol.Object]);

            _assemblyDefinition.MainModule.Types.Add(classType);
            _structs.Add(key, classType);
            _knownTypes.Add(key, classType);

            // Forward-declare empty constructor
            var emptyCtorDefinition = new MethodDefinition(".ctor",
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName |
                MethodAttributes.HideBySig,
                _knownTypes[TypeSymbol.Void]
            );

            classType.Methods.Insert(0, emptyCtorDefinition);

            // Forward-declare initializer constructor
            var defaultCtorDefinition = new MethodDefinition(".ctor",
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName |
                MethodAttributes.HideBySig,
                _knownTypes[TypeSymbol.Void]
            );

            // This constructor will be the second one on the class
            classType.Methods.Insert(1, defaultCtorDefinition);
        }

        private void EmitStructBody(ClassSymbol key, BoundBlockStatement value)
        {
            var structType = _structs[key];

            EmitEmptyConstructorForStruct(value, structType);
            EmitDefaultConstructorForStruct(key, value, structType);
        }

        private void EmitEmptyConstructorForStruct(BoundBlockStatement value, TypeDefinition structType)
        {
            // Create empty constructor
            var constructor = structType.Methods[0];
            var ilProcessor = constructor.Body.GetILProcessor();

            foreach (var field in value.Statements)
            {
                if (field is BoundVariableDeclaration d)
                {
                    var fieldAttributes = d.Variable.IsReadOnly ? FieldAttributes.Public | FieldAttributes.InitOnly : FieldAttributes.Public;
                    var fieldDefinition = new FieldDefinition(d.Variable.Name, fieldAttributes, _knownTypes[d.Variable.Type]);
                    structType.Fields.Add(fieldDefinition);

                    EmitFieldAssignment(ilProcessor, d, fieldDefinition);
                }
                else if (field is BoundSequencePointStatement s && s.Statement is BoundVariableDeclaration sd)
                {
                    var fieldAttributes = sd.Variable.IsReadOnly ? FieldAttributes.Public | FieldAttributes.InitOnly : FieldAttributes.Public;
                    var fieldDefinition = new FieldDefinition(sd.Variable.Name, fieldAttributes, _knownTypes[sd.Variable.Type]);
                    structType.Fields.Add(fieldDefinition);

                    EmitSequencePointStatement(ilProcessor, s);
                    // EmitFieldAssignment(ilProcessor, sd, fieldDefinition);
                }
                else
                {
                    throw new InternalCompilerException($"Unexpected statement type {field.Kind}. Expected BoundVariableDeclaration.");
                }
            }

            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Call, _objectCtor);
            ilProcessor.Emit(OpCodes.Ret);

            constructor.Body.Optimize();
        }

        private void EmitDefaultConstructorForStruct(ClassSymbol @class, BoundBlockStatement value, TypeDefinition structType)
        {
            // Create empty constructor
            var constructor = structType.Methods[1];
            var ilProcessor = constructor.Body.GetILProcessor();

            // Call base .ctor(), which should in turn call the object.ctor()
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Call, structType.Methods[0]);

            // Assign each parameter
            for (var i = 0; i < @class.CtorParameters.Length; i++)
            {
                var ctorParam = @class.CtorParameters[i];
                var paramType = _knownTypes[ctorParam.Type];
                const ParameterAttributes parameterAttributes = ParameterAttributes.None;
                var parameterDefinition = new ParameterDefinition(ctorParam.Name, parameterAttributes, paramType);

                constructor.Parameters.Add(parameterDefinition);

                ilProcessor.Emit(OpCodes.Ldarg_0);
                ilProcessor.Emit(OpCodes.Ldarg, i + 1);

                foreach (var field in structType.Fields)
                {
                    if (field.Name == ctorParam.Name)
                    {
                        ilProcessor.Emit(OpCodes.Stfld, field);
                        break;
                    }
                }
            }

            ilProcessor.Emit(OpCodes.Ret);

            constructor.Body.Optimize();
        }

        private void EmitFunctionDeclaration(FunctionSymbol function)
        {
            var functionType = _knownTypes[function.ReturnType];
            var methodAttributes = function.Receiver == null ? MethodAttributes.Static | MethodAttributes.Private : MethodAttributes.Public;
            var method = new MethodDefinition(function.Name, methodAttributes, functionType);

            foreach (var parameter in function.Parameters)
            {
                var parameterType = _knownTypes[parameter.Type];
                const ParameterAttributes parameterAttributes = ParameterAttributes.None;
                var parameterDefinition = new ParameterDefinition(parameter.Name, parameterAttributes, parameterType);

                method.Parameters.Add(parameterDefinition);
            }

            if (function.Receiver == null)
            {
                _typeDefinition.Methods.Add(method);
            }
            else
            {
                _structs[function.Receiver].Methods.Add(method);
            }

            _methods.Add(function, method);
        }

        private void EmitFunctionBody(FunctionSymbol function, BoundBlockStatement body)
        {
            var method = _methods[function];
            _locals.Clear();
            _labels.Clear();
            _fixups.Clear();

            var ilProcessor = method.Body.GetILProcessor();

            foreach (var statement in body.Statements)
            {
                EmitStatement(ilProcessor, statement);
            }

            foreach (var (InstructionIndex, Target) in _fixups)
            {
                var targetInstructionIndex = _labels[Target];
                var targetInstruction = ilProcessor.Body.Instructions[targetInstructionIndex];
                var instructionToFixup = ilProcessor.Body.Instructions[InstructionIndex];
                instructionToFixup.Operand = targetInstruction;
            }

            method.Body.Optimize();

            // TODO: Only emit this when emitting symbols

            method.DebugInformation.Scope = new ScopeDebugInformation(method.Body.Instructions[0], method.Body.Instructions.Last());

            foreach (var (symbol, definition) in _locals)
            {
                var debugInfo = new VariableDebugInformation(definition, symbol.Name);
                method.DebugInformation.Scope.Variables.Add(debugInfo);
            }
        }

        private void EmitStatement(ILProcessor ilProcessor, BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.NopStatement:
                    EmitNopStatement(ilProcessor, (BoundNopStatement)node);
                    break;
                case BoundNodeKind.VariableDeclaration:
                    EmitVariableDeclaration(ilProcessor, (BoundVariableDeclaration)node);
                    break;
                case BoundNodeKind.LabelStatement:
                    EmitLabelStatement(ilProcessor, (BoundLabelStatement)node);
                    break;
                case BoundNodeKind.GotoStatement:
                    EmitGotoStatement(ilProcessor, (BoundGotoStatement)node);
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    EmitConditionalGotoStatement(ilProcessor, (BoundConditionalGotoStatement)node);
                    break;
                case BoundNodeKind.ReturnStatement:
                    EmitReturnStatement(ilProcessor, (BoundReturnStatement)node);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EmitExpressionStatement(ilProcessor, (BoundExpressionStatement)node);
                    break;
                case BoundNodeKind.SequencePointStatement:
                    EmitSequencePointStatement(ilProcessor, (BoundSequencePointStatement)node);
                    break;
                default:
                    throw new InternalCompilerException($"Unexpected node kind {node.Kind}");
            }
        }

        private static void EmitNopStatement(ILProcessor ilProcessor, BoundNopStatement node)
        {
            ilProcessor.Emit(OpCodes.Nop);
        }

        private static void EmitFieldAssignment(ILProcessor ilProcessor, BoundVariableDeclaration node, FieldDefinition field)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);

            if (node.Initializer.ConstantValue != null)
            {
                EmitConstantExpression(ilProcessor, node.Initializer);
                ilProcessor.Emit(OpCodes.Stfld, field);
            }
        }

        private void EmitVariableDeclaration(ILProcessor ilProcessor, BoundVariableDeclaration node)
        {
            var typeReference = _knownTypes[node.Variable.Type];
            var variableDefinition = new VariableDefinition(typeReference);
            _locals.Add(node.Variable, variableDefinition);
            ilProcessor.Body.Variables.Add(variableDefinition);

            EmitExpression(ilProcessor, node.Initializer);
            ilProcessor.Emit(OpCodes.Stloc, variableDefinition);
        }

        private void EmitLabelStatement(ILProcessor ilProcessor, BoundLabelStatement node)
        {
            _labels.Add(node.Label, ilProcessor.Body.Instructions.Count);
        }

        private void EmitGotoStatement(ILProcessor ilProcessor, BoundGotoStatement node)
        {
            _fixups.Add((ilProcessor.Body.Instructions.Count, node.Label));
            ilProcessor.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
        }

        private void EmitConditionalGotoStatement(ILProcessor ilProcessor, BoundConditionalGotoStatement node)
        {
            EmitExpression(ilProcessor, node.Condition);

            var opCode = node.JumpIfTrue ? OpCodes.Brtrue : OpCodes.Brfalse;
            _fixups.Add((ilProcessor.Body.Instructions.Count, node.Label));
            ilProcessor.Emit(opCode, Instruction.Create(OpCodes.Nop));
        }

        private void EmitReturnStatement(ILProcessor ilProcessor, BoundReturnStatement node)
        {
            if (node.Expression != null)
            {
                EmitExpression(ilProcessor, node.Expression);
            }

            ilProcessor.Emit(OpCodes.Ret);
        }

        private void EmitExpressionStatement(ILProcessor ilProcessor, BoundExpressionStatement node)
        {
            EmitExpression(ilProcessor, node.Expression);

            if (node.Expression.Type != TypeSymbol.Void)
                ilProcessor.Emit(OpCodes.Pop);
        }

        private void EmitSequencePointStatement(ILProcessor ilProcessor, BoundSequencePointStatement node)
        {
            var index = ilProcessor.Body.Instructions.Count;
            EmitStatement(ilProcessor, node.Statement);

            var instruction = ilProcessor.Body.Instructions[index];

            if (!_documents.TryGetValue(node.Location.Text, out var document))
            {
                var fullPath = Path.GetFullPath(node.Location.Text.FileName);
                document = new Document(fullPath);
                _documents.Add(node.Location.Text, document);
            }

            var sequencePoint = new SequencePoint(instruction, document)
            {
                StartLine = node.Location.StartLine + 1,
                StartColumn = node.Location.StartCharacter + 1,
                EndLine = node.Location.EndLine + 1,
                EndColumn = node.Location.EndCharacter + 1
            };

            ilProcessor.Body.Method.DebugInformation.SequencePoints.Add(sequencePoint);
        }

        private void EmitExpression(ILProcessor ilProcessor, BoundExpression node)
        {
            if (node.ConstantValue != null)
            {
                EmitConstantExpression(ilProcessor, node);
                return;
            }

            switch (node.Kind)
            {
                case BoundNodeKind.AssignmentExpression:
                    EmitAssignmentExpression(ilProcessor, (BoundAssignmentExpression)node);
                    break;
                case BoundNodeKind.BinaryExpression:
                    EmitBinaryExpression(ilProcessor, (BoundBinaryExpression)node);
                    break;
                case BoundNodeKind.CallExpression:
                    EmitCallExpression(ilProcessor, (BoundCallExpression)node);
                    break;
                case BoundNodeKind.ConversionExpression:
                    EmitConversionExpression(ilProcessor, (BoundConversionExpression)node);
                    break;
                case BoundNodeKind.FieldAccessExpression:
                    EmitFieldAccessExpression(ilProcessor, (BoundFieldAccessExpression)node);
                    break;
                case BoundNodeKind.FieldAssignmentExpression:
                    EmitFieldAssignmentExpression(ilProcessor, (BoundFieldAssignmentExpression)node);
                    break;
                case BoundNodeKind.ThisExpression:
                    EmitThisExpression(ilProcessor, (BoundThisExpression)node);
                    break;
                case BoundNodeKind.UnaryExpression:
                    EmitUnaryExpression(ilProcessor, (BoundUnaryExpression)node);
                    break;
                case BoundNodeKind.VariableExpression:
                    EmitVariableExpression(ilProcessor, (BoundVariableExpression)node);
                    break;
                default:
                    throw new InternalCompilerException($"Unexpected node kind {node.Kind}");
            }
        }

        private void EmitThisExpression(ILProcessor ilProcessor, BoundThisExpression node)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
        }

        private void EmitFieldAssignmentExpression(ILProcessor ilProcessor, BoundFieldAssignmentExpression node)
        {
            var structSymbol = node.StructInstance.Type as ClassSymbol;

            Debug.Assert(structSymbol != null);

            EmitExpression(ilProcessor, node.StructInstance);
            EmitExpression(ilProcessor, node.Expression);

            var @struct = _structs[structSymbol];

            foreach (var field in @struct.Fields)
            {
                if (field.Name == node.StructMember.Name)
                {
                    ilProcessor.Emit(OpCodes.Stfld, field);
                    break;
                }
            }

            // Tmp variable that will get popped
            ilProcessor.Emit(OpCodes.Ldc_I4_0);
        }

        private void EmitFieldAccessExpression(ILProcessor ilProcessor, BoundFieldAccessExpression node)
        {
            EmitExpression(ilProcessor, node.StructInstance);

            var structSymbol = node.StructInstance.Type as ClassSymbol;

            Debug.Assert(structSymbol != null);

            var @struct = _structs[structSymbol];

            foreach (var field in @struct.Fields)
            {
                if (field.Name == node.StructMember.Name)
                {
                    ilProcessor.Emit(OpCodes.Ldfld, field);
                    break;
                }
            }
        }

        private static void EmitConstantExpression(ILProcessor ilProcessor, BoundExpression node)
        {
            Debug.Assert(node.ConstantValue != null);

            if (node.Type == TypeSymbol.Bool)
            {
                Debug.Assert(node.ConstantValue.Value != null);

                var value = (bool)node.ConstantValue.Value;
                var instruction = value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
                ilProcessor.Emit(instruction);
            }
            else if (node.Type == TypeSymbol.Int8 || node.Type == TypeSymbol.Int16 || node.Type == TypeSymbol.Int32)
            {
                Debug.Assert(node.ConstantValue.Value != null);

                var value = Convert.ToInt32(node.ConstantValue.Value);

                if (value <= int.MaxValue)
                {
                    ilProcessor.Emit(OpCodes.Ldc_I4, value);
                }
                else
                {
                    ilProcessor.Emit(OpCodes.Ldc_I4_S, value);
                }
            }
            else if (node.Type == TypeSymbol.UInt8 || node.Type == TypeSymbol.UInt16 || node.Type == TypeSymbol.UInt32)
            {
                Debug.Assert(node.ConstantValue.Value != null);

                var value = (uint)node.ConstantValue.Value;

                if (value <= int.MaxValue)
                {
                    ilProcessor.Emit(OpCodes.Ldc_I4, (int)value);
                }
                else
                {
                    ilProcessor.Emit(OpCodes.Ldc_I4_S, value);
                }
            }
            else if (node.Type == TypeSymbol.Int64)
            {
                Debug.Assert(node.ConstantValue.Value != null);

                var value = (long)node.ConstantValue.Value;
                if (value <= int.MaxValue)
                {
                    ilProcessor.Emit(OpCodes.Ldc_I4, (int)value);
                    ilProcessor.Emit(OpCodes.Conv_I8);
                }
                else
                {
                    ilProcessor.Emit(OpCodes.Ldc_I8, value);
                }
            }
            else if (node.Type == TypeSymbol.Float32)
            {
                Debug.Assert(node.ConstantValue.Value != null);

                var value = (float)node.ConstantValue.Value;
                ilProcessor.Emit(OpCodes.Ldc_R4, value);
            }
            else if (node.Type == TypeSymbol.Float64)
            {
                Debug.Assert(node.ConstantValue.Value != null);

                var value = (double)node.ConstantValue.Value;
                ilProcessor.Emit(OpCodes.Ldc_R8, value);
            }
            else if (node.Type == TypeSymbol.Char)
            {
                Debug.Assert(node.ConstantValue.Value != null);

                var value = Convert.ToInt32(node.ConstantValue.Value);
                ilProcessor.Emit(OpCodes.Ldc_I4, value);
            }

            // TODO: Handle decimal

            else if (node.Type == TypeSymbol.String)
            {
                // TODO: Handle null strings
                Debug.Assert(node.ConstantValue.Value != null);

                var value = (string)node.ConstantValue.Value;
                ilProcessor.Emit(OpCodes.Ldstr, value);
            }
            else
            {
                throw new InternalCompilerException($"Unexpected constant expression type: {node.Type}");
            }
        }

        private void EmitVariableExpression(ILProcessor ilProcessor, BoundVariableExpression node)
        {
            if (node.Variable is ParameterSymbol parameter)
            {
                ilProcessor.Emit(OpCodes.Ldarg, ilProcessor.Body.Method.HasThis ? parameter.Ordinal + 1 : parameter.Ordinal);
            }
            else
            {
                var variableDefinition = _locals[node.Variable];
                ilProcessor.Emit(OpCodes.Ldloc, variableDefinition);
            }
        }

        private void EmitAssignmentExpression(ILProcessor ilProcessor, BoundAssignmentExpression node)
        {
            var variableDefinition = _locals[node.Variable];
            EmitExpression(ilProcessor, node.Expression);
            ilProcessor.Emit(OpCodes.Dup);
            ilProcessor.Emit(OpCodes.Stloc, variableDefinition);
        }

        private void EmitUnaryExpression(ILProcessor ilProcessor, BoundUnaryExpression node)
        {
            EmitExpression(ilProcessor, node.Operand);

            if (node.Op.Kind == BoundUnaryOperatorKind.Identity)
            {
                // Done
            }
            else if (node.Op.Kind == BoundUnaryOperatorKind.LogicalNegation)
            {
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
            }
            else if (node.Op.Kind == BoundUnaryOperatorKind.Negation)
            {
                ilProcessor.Emit(OpCodes.Neg);
            }
            else if (node.Op.Kind == BoundUnaryOperatorKind.OnesComplement)
            {
                ilProcessor.Emit(OpCodes.Not);
            }
            else
            {
                throw new Exception($"Unexpected unary operator {SyntaxFacts.GetText(node.Op.SyntaxKind)}({node.Operand.Type})");
            }
        }

        private void EmitBinaryExpression(ILProcessor ilProcessor, BoundBinaryExpression node)
        {
            // +(string, string)

            if (node.Op.Kind == BoundBinaryOperatorKind.Addition)
            {
                if (node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
                {
                    EmitStringConcatExpression(ilProcessor, node);
                    return;
                }
            }

            EmitExpression(ilProcessor, node.Left);
            EmitExpression(ilProcessor, node.Right);

            // ==(object, object)
            // ==(string, string)

            if (node.Op.Kind == BoundBinaryOperatorKind.Equals)
            {
                if ((node.Left.Type == TypeSymbol.Object && node.Right.Type == TypeSymbol.Object) ||
                    (node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String))
                {
                    ilProcessor.Emit(OpCodes.Call, _objectEqualsReference);
                    return;
                }
            }

            // !=(object, object)
            // !=(string, string)

            if (node.Op.Kind == BoundBinaryOperatorKind.NotEquals)
            {
                if ((node.Left.Type == TypeSymbol.Object && node.Right.Type == TypeSymbol.Object) ||
                    (node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String))
                {
                    ilProcessor.Emit(OpCodes.Call, _objectEqualsReference);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    return;
                }
            }

            switch (node.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    ilProcessor.Emit(OpCodes.Add);
                    break;
                case BoundBinaryOperatorKind.Subtraction:
                    ilProcessor.Emit(OpCodes.Sub);
                    break;
                case BoundBinaryOperatorKind.Multiplication:
                    ilProcessor.Emit(OpCodes.Mul);
                    break;
                case BoundBinaryOperatorKind.Division:
                    ilProcessor.Emit(OpCodes.Div);
                    break;
                case BoundBinaryOperatorKind.Modulo:
                    ilProcessor.Emit(OpCodes.Rem);
                    break;
                
                // TODO: Implement short-circuit evaluation 
                case BoundBinaryOperatorKind.LogicalAnd:
                case BoundBinaryOperatorKind.BitwiseAnd:
                    ilProcessor.Emit(OpCodes.And);
                    break;
                
                // TODO: Implement short-circuit evaluation 
                case BoundBinaryOperatorKind.LogicalOr:
                case BoundBinaryOperatorKind.BitwiseOr:
                    ilProcessor.Emit(OpCodes.Or);
                    break;
                case BoundBinaryOperatorKind.BitwiseXor:
                    ilProcessor.Emit(OpCodes.Xor);
                    break;
                case BoundBinaryOperatorKind.Equals:
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperatorKind.NotEquals:
                    ilProcessor.Emit(OpCodes.Ceq);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperatorKind.Less:
                    ilProcessor.Emit(OpCodes.Clt);
                    break;
                case BoundBinaryOperatorKind.LessOrEquals:
                    ilProcessor.Emit(OpCodes.Cgt);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperatorKind.Greater:
                    ilProcessor.Emit(OpCodes.Cgt);
                    break;
                case BoundBinaryOperatorKind.GreaterOrEquals:
                    ilProcessor.Emit(OpCodes.Clt);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                default:
                    throw new InternalCompilerException($"Unexpected binary operator {SyntaxFacts.GetText(node.Op.SyntaxKind)}({node.Left.Type}, {node.Right.Type})");
            }
        }

        private void EmitStringConcatExpression(ILProcessor ilProcessor, BoundBinaryExpression node)
        {
            // Flatten the expression tree to a sequence of nodes to concatenate, then fold consecutive constants in that sequence.
            // This approach enables constant folding of non-sibling nodes, which cannot be done in the ConstantFolding class as it would require changing the tree.
            // Example: folding b and c in ((a + b) + c) if they are constant.

            var nodes = FoldConstants(node.Syntax, Flatten(node)).ToList();

            switch (nodes.Count)
            {
                case 0:
                    ilProcessor.Emit(OpCodes.Ldstr, string.Empty);
                    break;

                case 1:
                    EmitExpression(ilProcessor, nodes[0]);
                    break;

                case 2:
                    EmitExpression(ilProcessor, nodes[0]);
                    EmitExpression(ilProcessor, nodes[1]);
                    ilProcessor.Emit(OpCodes.Call, _stringConcat2Reference);
                    break;

                case 3:
                    EmitExpression(ilProcessor, nodes[0]);
                    EmitExpression(ilProcessor, nodes[1]);
                    EmitExpression(ilProcessor, nodes[2]);
                    ilProcessor.Emit(OpCodes.Call, _stringConcat3Reference);
                    break;

                case 4:
                    EmitExpression(ilProcessor, nodes[0]);
                    EmitExpression(ilProcessor, nodes[1]);
                    EmitExpression(ilProcessor, nodes[2]);
                    EmitExpression(ilProcessor, nodes[3]);
                    ilProcessor.Emit(OpCodes.Call, _stringConcat4Reference);
                    break;

                default:
                    ilProcessor.Emit(OpCodes.Ldc_I4, nodes.Count);
                    ilProcessor.Emit(OpCodes.Newarr, _knownTypes[TypeSymbol.String]);

                    for (var i = 0; i < nodes.Count; i++)
                    {
                        ilProcessor.Emit(OpCodes.Dup);
                        ilProcessor.Emit(OpCodes.Ldc_I4, i);
                        EmitExpression(ilProcessor, nodes[i]);
                        ilProcessor.Emit(OpCodes.Stelem_Ref);
                    }

                    ilProcessor.Emit(OpCodes.Call, _stringConcatArrayReference);
                    break;
            }

            // (a + b) + (c + d) --> [a, b, c, d]
            static IEnumerable<BoundExpression> Flatten(BoundExpression node)
            {
                if (node is BoundBinaryExpression binaryExpression &&
                    binaryExpression.Op.Kind == BoundBinaryOperatorKind.Addition &&
                    binaryExpression.Left.Type == TypeSymbol.String &&
                    binaryExpression.Right.Type == TypeSymbol.String)
                {
                    foreach (var result in Flatten(binaryExpression.Left))
                    {
                        yield return result;
                    }

                    foreach (var result in Flatten(binaryExpression.Right))
                    {
                        yield return result;
                    }
                }
                else
                {
                    if (node.Type != TypeSymbol.String)
                    {
                        throw new Exception($"Unexpected node type in string concatenation: {node.Type}");
                    }

                    yield return node;
                }
            }

            // [a, "foo", "bar", b, ""] --> [a, "foobar", b]
            static IEnumerable<BoundExpression> FoldConstants(SyntaxNode syntax, IEnumerable<BoundExpression> nodes)
            {
                StringBuilder? sb = null;

                foreach (var node in nodes)
                {
                    if (node.ConstantValue != null)
                    {
                        var stringValue = (string)node.ConstantValue.Value!;

                        if (string.IsNullOrEmpty(stringValue))
                        {
                            continue;
                        }

                        sb ??= new StringBuilder();
                        sb.Append(stringValue);
                    }
                    else
                    {
                        if (sb?.Length > 0)
                        {
                            yield return new BoundLiteralExpression(syntax, sb.ToString());
                            sb.Clear();
                        }

                        yield return node;
                    }
                }

                if (sb?.Length > 0)
                {
                    yield return new BoundLiteralExpression(syntax, sb.ToString());
                }
            }
        }

        private void EmitCallExpression(ILProcessor ilProcessor, BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Rnd)
            {
                if (_randomFieldDefinition == null)
                {
                    EmitRandomField();
                }

                ilProcessor.Emit(OpCodes.Ldsfld, _randomFieldDefinition);

                foreach (var argument in node.Arguments)
                {
                    EmitExpression(ilProcessor, argument);
                }

                ilProcessor.Emit(OpCodes.Callvirt, _randomNextReference);
                return;
            }

            if (node.Instance != null)
            {
                var methodDefinition = _methods[node.Function];

                if (node.Instance is BoundVariableExpression variable)
                {
                    EmitVariableExpression(ilProcessor, variable);
                }
                else if (node.Instance is BoundFieldAccessExpression field)
                {
                    EmitFieldAccessExpression(ilProcessor, field);
                }
                else if (node.Instance is BoundThisExpression instance)
                {
                    EmitThisExpression(ilProcessor, instance);
                }
                else
                {
                    throw new Exception("Unexpected node type in call expression");
                }

                foreach (var argument in node.Arguments)
                {
                    EmitExpression(ilProcessor, argument);
                }

                ilProcessor.Emit(OpCodes.Callvirt, methodDefinition);
            }
            else
            {
                foreach (var argument in node.Arguments)
                {
                    EmitExpression(ilProcessor, argument);
                }

                if (node.Function == BuiltinFunctions.ReadLine)
                {
                    ilProcessor.Emit(OpCodes.Call, _consoleReadLineReference);
                }
                else if (node.Function == BuiltinFunctions.ReadKey)
                {
                    ilProcessor.Emit(OpCodes.Call, _consoleReadKeyReference);
                }
                else if (node.Function == BuiltinFunctions.WriteLine)
                {
                    ilProcessor.Emit(OpCodes.Call, _consoleWriteLineReference);
                }
                else if (node.Function == BuiltinFunctions.Write)
                {
                    ilProcessor.Emit(OpCodes.Call, _consoleWriteReference);
                }
                else if (node.Function == BuiltinFunctions.ReadAllText)
                {
                    ilProcessor.Emit(OpCodes.Call, _readAllTextReference);
                }
                else if (node.Function == BuiltinFunctions.WriteAllText)
                {
                    ilProcessor.Emit(OpCodes.Call, _writeAllTextReference);
                }
                else if (node.Function.Name.EndsWith(".ctor"))
                {
                    var className = node.Function.Name[..^5];
                    var @struct = _structs.First(s => s.Key.Name == className).Value;

                    // TODO: Use a general overload resolution algorithm instead
                    ilProcessor.Emit(OpCodes.Newobj, node.Arguments.Length == 0 ? @struct.Methods[0] : @struct.Methods[1]);
                }
                else
                {
                    var methodDefinition = _methods[node.Function];
                    ilProcessor.Emit(OpCodes.Call, methodDefinition);
                }
            }
        }

        private void EmitRandomField()
        {
            _randomFieldDefinition = new FieldDefinition(
                "$rnd",
                FieldAttributes.Static | FieldAttributes.Private,
                _randomReference
            );
            _typeDefinition.Fields.Add(_randomFieldDefinition);

            var staticConstructor = new MethodDefinition(
                ".cctor",
                MethodAttributes.Static |
                MethodAttributes.Private |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName,
                _knownTypes[TypeSymbol.Void]
            );
            _typeDefinition.Methods.Insert(0, staticConstructor);

            var ilProcessor = staticConstructor.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Newobj, _randomCtorReference);
            ilProcessor.Emit(OpCodes.Stsfld, _randomFieldDefinition);
            ilProcessor.Emit(OpCodes.Ret);
        }

        private void EmitConversionExpression(ILProcessor ilProcessor, BoundConversionExpression node)
        {
            EmitExpression(ilProcessor, node.Expression);
            var needsBoxing = node.Expression.Type == TypeSymbol.Bool ||
                              node.Expression.Type.IsNumeric;

            if (needsBoxing)
            {
                ilProcessor.Emit(OpCodes.Box, _knownTypes[node.Expression.Type]);
            }

            if (node.Type == TypeSymbol.Object)
            {
                // Done
            }
            else if (node.Type == TypeSymbol.Bool)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToBooleanReference);
            }
            else if (node.Type == TypeSymbol.Int8)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToInt8Reference);
            }
            else if (node.Type == TypeSymbol.Int16)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToInt16Reference);
            }
            else if (node.Type == TypeSymbol.Int32)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToInt32Reference);
            }
            else if (node.Type == TypeSymbol.Int64)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToInt64Reference);
            }
            else if (node.Type == TypeSymbol.UInt8)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToUInt8Reference);
            }
            else if (node.Type == TypeSymbol.UInt16)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToUInt16Reference);
            }
            else if (node.Type == TypeSymbol.UInt32)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToUInt32Reference);
            }
            else if (node.Type == TypeSymbol.UInt64)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToUInt64Reference);
            }
            else if (node.Type == TypeSymbol.Float32)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToFloat32Reference);
            }
            else if (node.Type == TypeSymbol.Float64)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToFloat64Reference);
            }
            else if (node.Type == TypeSymbol.Decimal)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToFloat128Reference);
            }
            else if (node.Type == TypeSymbol.Char)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToCharReference);
            }
            else if (node.Type == TypeSymbol.String)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToStringReference);
            }
            else
            {
                throw new InternalCompilerException($"Unexpected conversion from {node.Expression.Type} to {node.Type}");
            }
        }
    }
}