using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Vivian.CodeAnalysis.Binding;
using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Emit
{
    internal static class Emitter
    {
        public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
        {
            if (program.Diagnostics.Any())
            {
                return program.Diagnostics;
            }

            var assemblies = new List<AssemblyDefinition>();
            var result = new DiagnosticBag();

            foreach (var reference in references)
            {
                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly(reference);
                    assemblies.Add(assembly);
                }
                catch (BadImageFormatException)
                {
                    result.ReportInvalidReference(reference);
                }
            }

            // Resolve Types
            // Object -> System.Object
            // Int -> System.Int32
            // String -> System.String
            // Void -> System.Void
            // Bool -> System.Boolean

            var builtInTypes = new List<(TypeSymbol type, string MetadataName)>()
            {
                (TypeSymbol.Object, "System.Object"),
                (TypeSymbol.Bool, "System.Boolean"),
                (TypeSymbol.Int, "System.Int32"),
                (TypeSymbol.String, "System.String"),
                (TypeSymbol.Void, "System.Void"),
            };

            var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
            var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);
            var knownTypes = new Dictionary<TypeSymbol, TypeReference>();
            
            foreach (var (typeSymbol, metadataName) in builtInTypes)
            {
                var foundTypes = assemblies.SelectMany(a => a.Modules)
                                           .SelectMany(m => m.Types)
                                           .Where(t => t.FullName == metadataName)
                                           .ToArray();

                if (foundTypes.Length == 1)
                {
                    var typeReference = assemblyDefinition.MainModule.ImportReference(foundTypes[0]);
                    knownTypes.Add(typeSymbol, typeReference);
                }
                
                else if (foundTypes.Length == 0)
                {
                    result.ReportRequiredTypeNotFound(typeSymbol.Name, metadataName);
                }
                else
                {
                    result.ReportRequiredTypeAmbiguous(typeSymbol.Name, metadataName, foundTypes);
                }
            }
            
            if (result.Any())
            {
                return result.ToImmutableArray();
            }

            var objectType = knownTypes[TypeSymbol.Object];
            var typeDefinition = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, objectType); 
            assemblyDefinition.MainModule.Types.Add(typeDefinition);
            
            var voidType = knownTypes[TypeSymbol.Void];
            var mainMethod = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Private , voidType);
            typeDefinition.Methods.Add(mainMethod);

            var ilProcessor = mainMethod.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Ret);
            
            assemblyDefinition.EntryPoint = mainMethod;
            assemblyDefinition.Write(outputPath);

            return result.ToImmutableArray();
        }
    }
}