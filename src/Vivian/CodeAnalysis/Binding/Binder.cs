using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vivian.CodeAnalysis.Lowering;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;
using Vivian.CodeAnalysis.Text;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly FunctionSymbol? _function;

        private readonly Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> _loopStack = new Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)>();
        private int _labelCounter;
        private BoundScope _scope;

        private Binder(BoundScope? parent, FunctionSymbol? function)
        {
            _scope = new BoundScope(parent);
            _function = function;

            if (function != null)
            {
                foreach (var parameter in function.Parameters)
                {
                    _scope.TryDeclareVariable(parameter);
                }
            }
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, ImmutableArray<SyntaxTree> syntaxTrees)
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(parentScope, function: null);

            binder.Diagnostics.AddRange(syntaxTrees.SelectMany(st => st.Diagnostics));

            if (binder.Diagnostics.Any())
            {
                return new BoundGlobalScope(
                    previous,
                    binder.Diagnostics.ToImmutableArray(),
                    null,
                    null,
                    ImmutableArray<StructSymbol>.Empty,
                    ImmutableArray<FunctionSymbol>.Empty,
                    ImmutableArray<VariableSymbol>.Empty,
                    ImmutableArray<BoundStatement>.Empty);
            }

            // Phase 1: Forward declare structs
            var structDeclarations = syntaxTrees.SelectMany(st => st.Root.Members)
                                               .OfType<StructDeclarationSyntax>();

            foreach (var @struct in structDeclarations)
            {
                binder.BindStructDeclaration(@struct);
            }

            // Phase 2: Forward declare functions
            var functionDeclarations = syntaxTrees.SelectMany(st => st.Root.Members)
                                                  .OfType<FunctionDeclarationSyntax>();

            foreach (var function in functionDeclarations)
            {
                binder.BindFunctionDeclaration(function);
            }

            // Phase 3: Bind all global statements
            var globalStatements = syntaxTrees.SelectMany(st => st.Root.Members)
                                              .OfType<GlobalStatementSyntax>();

            var statements = ImmutableArray.CreateBuilder<BoundStatement>();

            var globalStatementSyntax = globalStatements.ToList();
            foreach (var globalStatement in globalStatementSyntax)
            {
                var statement = binder.BindGlobalStatement(globalStatement.Statement);
                statements.Add(statement);
            }

            // Check global statements
            var firstGlobalStatementPerSyntaxTree = syntaxTrees.Select(st => st.Root.Members.OfType<GlobalStatementSyntax>().FirstOrDefault())
                                                                .Where(g => g != null)
                                                                .ToArray();

            if (firstGlobalStatementPerSyntaxTree.Length > 1)
            {
                foreach (var globalStatement in firstGlobalStatementPerSyntaxTree)
                    binder.Diagnostics.ReportOnlyOneFileCanHaveGlobalStatements(globalStatement!.Location);
            }

            // Check for main/script with global statements
            var functions = binder._scope.GetDeclaredFunctions();

            FunctionSymbol? mainFunction = functions.FirstOrDefault(f => f.Name == "Main");
            
            FunctionSymbol? scriptFunction = null;

            if (mainFunction != null)
            {
                if (mainFunction.ReturnType != TypeSymbol.Void || mainFunction.Parameters.Any())
                {
                    binder.Diagnostics.ReportMainMustHaveCorrectSignature(mainFunction.Declaration!.Identifier.Location);
                }
            }

            if (globalStatementSyntax.Any())
            {
                if (mainFunction != null)
                {
                    binder.Diagnostics.ReportCannotMixMainAndGlobalStatements(mainFunction.Declaration!.Identifier.Location);

                    foreach (var globalStatement in firstGlobalStatementPerSyntaxTree)
                        binder.Diagnostics.ReportCannotMixMainAndGlobalStatements(globalStatement!.Location);
                }
                else
                {
                    mainFunction = new FunctionSymbol("Main", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void, null);
                }
            }

            var diagnostics = binder.Diagnostics.ToImmutableArray();
            var variables = binder._scope.GetDeclaredVariables();
            var structs = binder._scope.GetDeclaredStructs();

            if (previous != null)
            {
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
            }

            return new BoundGlobalScope(previous, diagnostics, mainFunction, scriptFunction, structs, functions, variables, statements.ToImmutable());
        }

        public static BoundProgram BindProgram(BoundProgram? previous, BoundGlobalScope globalScope)
        {
            var parentScope = CreateParentScope(globalScope);

            if (globalScope.Diagnostics.Any())
            {
                return new BoundProgram(
                    previous,
                    globalScope.Diagnostics,
                    null,
                    null,
                    ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Empty,
                    ImmutableDictionary<StructSymbol, BoundBlockStatement>.Empty);
            }

            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var structBodies = ImmutableDictionary.CreateBuilder<StructSymbol, BoundBlockStatement>();
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            foreach (var @struct in globalScope.Structs)
            {
                var binder = new Binder(parentScope, null);
                var body = binder.BindMemberBlockStatement(@struct.Declaration!.Body);
                var loweredBody = Lowerer.Lower(@struct, body);

                structBodies.Add(@struct, loweredBody);

                diagnostics.AddRange(binder.Diagnostics);
            }

            foreach (var function in globalScope.Functions)
            {
                if (function.ReturnType is StructSymbol) { continue; }

                var binder = new Binder(parentScope, function);
                var body = binder.BindStatement(function.Declaration!.Body);
                var loweredBody = Lowerer.Lower(function, body);

                if (function.ReturnType != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                {
                    binder.Diagnostics.ReportAllPathsMustReturn(function.Declaration.Identifier.Location);
                }

                functionBodies.Add(function, loweredBody);

                diagnostics.AddRange(binder.Diagnostics);
            }

            var compilationUnit = globalScope.Statements.Any()
                                    ? globalScope.Statements[0].Syntax.AncestorsAndSelf().LastOrDefault()
                                    : null;

            if (globalScope.MainFunction != null && globalScope.Statements.Any())
            {
                var body = Lowerer.Lower(globalScope.MainFunction, new BoundBlockStatement(compilationUnit!, globalScope.Statements));
                functionBodies.Add(globalScope.MainFunction, body);
            }
            else if (globalScope.ScriptFunction != null)
            {
                var statements = globalScope.Statements;

                if (statements.Length == 1 &&
                    statements[0] is BoundExpressionStatement es &&
                    es.Expression.Type != TypeSymbol.Void)
                {
                    statements = statements.SetItem(0, new BoundReturnStatement(es.Expression.Syntax, es.Expression));
                }
                else if (statements.Any() && statements.Last().Kind != BoundNodeKind.ReturnStatement)
                {
                    var nullValue = new BoundLiteralExpression(compilationUnit!, "");
                    statements = statements.Add(new BoundReturnStatement(compilationUnit!, nullValue));
                }

                var body = Lowerer.Lower(globalScope.ScriptFunction, new BoundBlockStatement(compilationUnit!, statements));
                functionBodies.Add(globalScope.ScriptFunction, body);
            }

            return new BoundProgram(previous,
                                    diagnostics.ToImmutable(),
                                    globalScope.MainFunction,
                                    globalScope.ScriptFunction,
                                    functionBodies.ToImmutable(),
                                    structBodies.ToImmutable());
        }

        private void BindStructDeclaration(StructDeclarationSyntax syntax)
        {
            // Peek into the struct body and generate a constructor based on all writeable members
            var members = syntax.Body.Statement.OfType<VariableDeclarationSyntax>();

            var ctorParameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var boundMembers = ImmutableArray.CreateBuilder<VariableSymbol>();

            foreach (var varDeclarationSyntax in members)
            {
                var declaration = BindVariableDeclaration(varDeclarationSyntax);

                if (declaration is BoundVariableDeclaration d)
                {
                    boundMembers.Add(d.Variable);
                }

                if (varDeclarationSyntax.Keyword.Kind == SyntaxKind.ConstKeyword)
                {
                    // These are not candidates for ctorParameters because they are read-only
                    continue;
                }

                var parameterName = varDeclarationSyntax.Identifier.Text;
                var parameterType = BindTypeClause(varDeclarationSyntax.TypeClause);

                if (parameterType == null && varDeclarationSyntax.Initializer != null)
                {
                    parameterType = BindExpression(varDeclarationSyntax.Initializer).Type;
                }
                else if (parameterType == null && varDeclarationSyntax.Initializer == null)
                {
                    // We don't need to do error reporting here (e.g. report on duplicate members) because that will be
                    // done later in the BindMemberBlockStatement
                    continue;
                }

                var parameter = new ParameterSymbol(parameterName, parameterType!, ctorParameters.Count);

                ctorParameters.Add(parameter);
            }

            string structIdentifier = syntax.Identifier.Text;
            var @struct = new StructSymbol(structIdentifier, ctorParameters.ToImmutable(), boundMembers.ToImmutable(), syntax);

            if (structIdentifier != null! && !_scope.TryDeclareStruct(@struct))
            {
                Diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Location, @struct.Name);
            }

            // Declare Built-in Constructors
            var ctor = new FunctionSymbol(structIdentifier + ".ctor", ImmutableArray<ParameterSymbol>.Empty, @struct);
            var ctorWithParams = new FunctionSymbol(structIdentifier + ".ctor", ctorParameters.ToImmutable(), @struct, overloadFor: ctor);

            if (structIdentifier != null && !_scope.TryDeclareFunction(ctorWithParams))
            {
                Diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Location, ctor.Name);
            }
        }

        private void BindFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameterNames = new HashSet<string>();

            foreach (var parameterSyntax in syntax.Parameters)
            {
                var parameterName = parameterSyntax.Identifier.Text;
                var parameterType = BindTypeClause(parameterSyntax.Type);

                if (!seenParameterNames.Add(parameterName))
                {
                    Diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Location, parameterName);
                }
                else
                {
                    var parameter = new ParameterSymbol(parameterName, parameterType!, parameters.Count);

                    parameters.Add(parameter);
                }
            }

            var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;
            var receiver = BindTypeClause(syntax.Receiver) as StructSymbol;

            var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax, receiver: receiver);

            if (syntax.Identifier.Text != null! && !_scope.TryDeclareFunction(function))
            {
                Diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Location, function.Name);
            }
        }

        private static BoundScope CreateParentScope(BoundGlobalScope? previous)
        {
            var stack = new Stack<BoundGlobalScope>();

            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            var parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();

                var scope = new BoundScope(parent);

                foreach (var s in previous.Structs)
                {
                    scope.TryDeclareStruct(s);
                }

                foreach (var f in previous.Functions)
                {
                    scope.TryDeclareFunction(f);
                }

                foreach (var v in previous.Variables)
                {
                    scope.TryDeclareVariable(v);
                }

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null);

            foreach (var f in BuiltinFunctions.GetAll())
            {
                result.TryDeclareFunction(f);
            }

            return result;
        }

        public DiagnosticBag Diagnostics { get; } = new();

        private static BoundStatement BindErrorStatement(SyntaxNode syntax)
        {
            return new BoundExpressionStatement(syntax, new BoundErrorExpression(syntax));
        }

        private BoundStatement BindGlobalStatement(StatementSyntax syntax)
        {
            return BindStatement(syntax, isGlobal: true);
        }

        private BoundStatement BindStatement(StatementSyntax syntax, bool isGlobal = false)
        {
            var result = BindStatementInternal(syntax);

            if (!isGlobal)
            {
                if (result is BoundExpressionStatement es)
                {
                    var isAllowedExpression = es.Expression.Kind == BoundNodeKind.ErrorExpression ||
                                              es.Expression.Kind == BoundNodeKind.AssignmentExpression ||
                                              es.Expression.Kind == BoundNodeKind.FieldAssignmentExpression ||
                                              es.Expression.Kind == BoundNodeKind.CallExpression ||
                                              es.Expression.Kind == BoundNodeKind.CompoundAssignmentExpression ||
                                              es.Expression.Kind == BoundNodeKind.CompoundFieldAssignmentExpression;

                    if (!isAllowedExpression)
                    {
                        Diagnostics.ReportInvalidExpressionStatement(syntax.Location);
                    }
                }
            }

            return result;
        }

        private BoundStatement BindMemberBlockStatement(MemberBlockStatementSyntax syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);

            foreach (var statementSyntax in syntax.Statement)
            {
                if (statementSyntax.Kind != SyntaxKind.VariableDeclaration)
                {
                    Diagnostics.ReportInvalidExpressionStatement(statementSyntax.Location);
                }

                var statement = BindVariableDeclaration((VariableDeclarationSyntax)statementSyntax);
                statements.Add(statement);
            }

            return new BoundMemberBlockStatement(syntax, statements.ToImmutable());
        }

        private BoundStatement BindStatementInternal(StatementSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BlockStatement:
                    return BindBlockStatement((BlockStatementSyntax) syntax);
                case SyntaxKind.VariableDeclaration:
                    return BindVariableDeclaration((VariableDeclarationSyntax) syntax);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfStatementSyntax) syntax);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileStatementSyntax) syntax);
                case SyntaxKind.DoWhileStatement:
                    return BindDoWhileStatement((DoWhileStatementSyntax) syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForStatementSyntax) syntax);
                case SyntaxKind.BreakStatement:
                    return BindBreakStatement((BreakStatementSyntax) syntax);
                case SyntaxKind.ContinueStatement:
                    return BindContinueStatement((ContinueStatementSyntax) syntax);
                case SyntaxKind.ReturnStatement:
                    return BindReturnStatement((ReturnStatementSyntax) syntax);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax) syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);

            foreach (var statementSyntax in syntax.Statements)
            {
                var statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }

            _scope = _scope.Parent!;

            return new BoundBlockStatement(syntax, statements.ToImmutable());
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var isReadOnly = syntax.Keyword.Kind == SyntaxKind.ConstKeyword;
            var type = BindTypeClause(syntax.TypeClause);

            if (syntax.Initializer != null && syntax.Initializer.Kind != SyntaxKind.DefaultKeyword)
            {
                var initializer = BindExpression(syntax.Initializer);
                var variableType = type ?? initializer.Type;

                if (initializer is BoundLiteralExpression ble && variableType.IsNumeric && ble.Type != variableType)
                {
                    // Check for over/underflows and adjust type
                    object? newValue = null;

                    if (variableType == TypeSymbol.Int8)
                    {
                        newValue = Convert.ToSByte(ble.Value);
                    }
                    else if (variableType == TypeSymbol.Int16)
                    {
                        newValue = Convert.ToInt16(ble.Value);
                    }
                    else if (variableType == TypeSymbol.Int32)
                    {
                        newValue = Convert.ToInt32(ble.Value);
                    }
                    else if (variableType == TypeSymbol.Int64)
                    {
                        newValue = Convert.ToInt64(ble.Value);
                    }
                    else if (variableType == TypeSymbol.UInt8)
                    {
                        newValue = Convert.ToByte(ble.Value);
                    }
                    else if (variableType == TypeSymbol.UInt16)
                    {
                        newValue = Convert.ToUInt16(ble.Value);
                    }
                    else if (variableType == TypeSymbol.UInt32)
                    {
                        newValue = Convert.ToUInt32(ble.Value);
                    }
                    else if (variableType == TypeSymbol.UInt64)
                    {
                        newValue = Convert.ToUInt64(ble.Value);
                    }
                    else if (variableType == TypeSymbol.Float32)
                    {
                        newValue = Convert.ToSingle(ble.Value);
                    }
                    else if (variableType == TypeSymbol.Float64)
                    {
                        newValue = Convert.ToDouble(ble.Value);
                    }
                    else if (variableType == TypeSymbol.Decimal)
                    {
                        newValue = Convert.ToDecimal(ble.Value);
                    }

                    initializer = new BoundLiteralExpression(syntax.Initializer, newValue);
                }

                var variable = BindVariableDeclaration(syntax.Identifier, isReadOnly, variableType, initializer.ConstantValue);
                var convertedInitializer = BindConversion(syntax.Initializer.Location, initializer, variableType);

                return new BoundVariableDeclaration(syntax, variable, convertedInitializer);
            }
            else if (type != null)
            {
                var initializer = syntax.Initializer?.Kind == SyntaxKind.DefaultKeyword
                    ? BindDefaultExpression((DefaultKeywordSyntax)syntax.Initializer, syntax.TypeClause)
                    : BindSyntheticDefaultExpression(syntax, syntax.TypeClause);
                var variable = BindVariableDeclaration(syntax.Identifier, isReadOnly, type);
                var convertedInitializer = BindConversion(syntax.TypeClause!.Location, initializer!, type);

                return new BoundVariableDeclaration(syntax, variable, convertedInitializer);
            }
            else
            {
                Diagnostics.ReportUndefinedType(syntax.TypeClause?.Location ?? syntax.Identifier.Location, syntax.TypeClause?.Identifier.Text ?? syntax.Identifier.Text);
                return new BoundExpressionStatement(syntax, new BoundErrorExpression(syntax));
            }
        }

        [return: NotNullIfNotNull("typeSyntax")]
        private BoundExpression? BindSyntheticDefaultExpression(VariableDeclarationSyntax syntax, TypeClauseSyntax? typeSyntax)
        {
            var syntaxToken = new SyntaxToken(syntax.SyntaxTree, SyntaxKind.DefaultKeyword, syntax.Span.End, null, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
            var syntaxNode = new DefaultKeywordSyntax(syntax.SyntaxTree, syntaxToken);

            return BindDefaultExpression(syntaxNode, typeSyntax);
        }

        [return: NotNullIfNotNull("typeSyntax")]
        private BoundExpression? BindDefaultExpression(DefaultKeywordSyntax syntax, TypeClauseSyntax? typeSyntax)
        {
            if (typeSyntax == null)
            {
                return null;
            }

            var type = LookupType(typeSyntax.Identifier.Text);

            if (type == null)
            {
                Diagnostics.ReportUndefinedType(typeSyntax.Identifier.Location, typeSyntax.Identifier.Text);
            }

            if (type is StructSymbol s)
            {
                // Struct types default to calling their empty constructor
                var ctorSyntaxToken = new NameExpressionSyntax(syntax.SyntaxTree, new SyntaxToken(syntax.SyntaxTree, SyntaxKind.IdentifierToken, syntax.Span.End, s.Name, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty));
                var openParenToken = new SyntaxToken(syntax.SyntaxTree, SyntaxKind.OpenParenthesisToken, syntax.Span.End, "(", null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
                var closeParenToken = new SyntaxToken(syntax.SyntaxTree, SyntaxKind.CloseParenthesisToken, syntax.Span.End, ")", null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);

                return BindCallExpression(new CallExpressionSyntax(syntax.SyntaxTree, ctorSyntaxToken, openParenToken, new SeparatedSyntaxList<ExpressionSyntax>(ImmutableArray<SyntaxNode>.Empty), closeParenToken));
            }

            return new BoundLiteralExpression(syntax, type?.DefaultValue);
        }

        private TypeSymbol? BindTypeClause(TypeClauseSyntax? syntax)
        {
            if (syntax == null)
            {
                return null;
            }

            var type = LookupType(syntax.Identifier.Text);

            if (type == null)
            {
                Diagnostics.ReportUndefinedType(syntax.Identifier.Location, syntax.Identifier.Text);
            }

            return type;
        }

        private TypeSymbol? BindTypeClause(SyntaxToken? identifier)
        {
            if (identifier == null || string.IsNullOrWhiteSpace(identifier.Text))
            {
                return null;
            }

            var type = LookupType(identifier.Text);

            if (type == null)
            {
                Diagnostics.ReportUndefinedType(identifier.Location, identifier.Text);
            }

            return type;
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);

            if (condition.ConstantValue != null)
            {
                if ((bool)condition.ConstantValue.Value! == false)
                {
                    Diagnostics.ReportUnreachableCode(syntax.ThenStatement);
                }
                else if (syntax.ElseClause != null)
                {
                    Diagnostics.ReportUnreachableCode(syntax.ElseClause.ElseStatement);
                }
            }

            var thenStatement = BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(syntax, condition, thenStatement, elseStatement);
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);

            if (condition.ConstantValue != null)
            {
                if (!(bool)condition.ConstantValue.Value!)
                {
                    Diagnostics.ReportUnreachableCode(syntax.Body);
                }
            }

            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundWhileStatement(syntax, condition, body, breakLabel, continueLabel);
        }

        private BoundStatement BindDoWhileStatement(DoWhileStatementSyntax syntax)
        {
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            return new BoundDoWhileStatement(syntax, body, condition, breakLabel, continueLabel);
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int32);
            var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int32);

            _scope = new BoundScope(_scope);

            var variable = BindVariableDeclaration(syntax.Identifier, isReadOnly: true, TypeSymbol.Int32);
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

            _scope = _scope.Parent!;

            return new BoundForStatement(syntax, variable, lowerBound, upperBound, body, breakLabel, continueLabel);
        }

        private BoundStatement BindLoopBody(StatementSyntax body, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            _labelCounter++;
            breakLabel = new BoundLabel($"break{_labelCounter}");
            continueLabel = new BoundLabel($"continue{_labelCounter}");

            _loopStack.Push((breakLabel, continueLabel));
            var boundBody = BindStatement(body);
            _loopStack.Pop();

            return boundBody;
        }

        private BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
        {
            if (_loopStack.Count == 0)
            {
                Diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement(syntax);
            }

            var breakLabel = _loopStack.Peek().BreakLabel;
            return new BoundGotoStatement(syntax, breakLabel);
        }

        private BoundStatement BindContinueStatement(ContinueStatementSyntax syntax)
        {
            if (_loopStack.Count == 0)
            {
                Diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement(syntax);
            }

            var continueLabel = _loopStack.Peek().ContinueLabel;
            return new BoundGotoStatement(syntax, continueLabel);
        }

        private BoundStatement BindReturnStatement(ReturnStatementSyntax syntax)
        {
            var expression = syntax.Expression == null ? null : BindExpression(syntax.Expression);

            if (_function == null)
            {
                if (expression != null)
                {
                    // Main does not support return values.
                    Diagnostics.ReportInvalidReturnWithValueInGlobalStatements(syntax.Expression!.Location);
                }
            }
            else
            {
                if (_function.ReturnType == TypeSymbol.Void)
                {
                    if (expression != null)
                    {
                        Diagnostics.ReportInvalidReturnExpression(syntax.Expression!.Location, _function.Name);
                    }
                }
                else
                {
                    if (expression == null)
                    {
                        Diagnostics.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, _function.ReturnType);
                    }
                    else
                    {
                        expression = BindConversion(syntax.Expression!.Location, expression, _function.ReturnType);
                    }
                }
            }

            return new BoundReturnStatement(syntax, expression);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(syntax, expression);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType)
        {
            return BindConversion(syntax, targetType);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var result = BindExpressionInternal(syntax);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                Diagnostics.ReportExpressionMustHaveValue(syntax.Location);
                return new BoundErrorExpression(syntax);
            }

            return result;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedExpressionSyntax) syntax);
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax) syntax);
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax) syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax) syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax) syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax) syntax);
                case SyntaxKind.CallExpression:
                    return BindCallExpression((CallExpressionSyntax) syntax);
                case SyntaxKind.MemberAccessExpression:
                    return BindMemberAccessExpression((MemberAccessExpressionSyntax) syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private static BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(syntax, value);
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            if (syntax.IdentifierToken.IsMissing)
            {
                // This means the token was inserted by the parser. We already
                // reported error so we can just return an error expression.
                return new BoundErrorExpression(syntax);
            }

            var variable = BindVariableReference(syntax.IdentifierToken);

            if (variable == null)
            {
                return new BoundErrorExpression(syntax);
            }

            return new BoundVariableExpression(syntax, variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            var variable = BindVariableReference(syntax.IdentifierToken);

            if (variable == null)
            {
                return boundExpression;
            }

            if (variable.IsReadOnly)
            {
                Diagnostics.ReportCannotAssign(syntax.AssignmentToken.Location, name);
            }

            if (syntax.AssignmentToken.Kind != SyntaxKind.EqualsToken)
            {
                var equivalentOperatorTokenKind = SyntaxFacts.GetBinaryOperatorOfAssignmentOperator(syntax.AssignmentToken.Kind);
                var boundOperator = BoundBinaryOperator.Bind(equivalentOperatorTokenKind, variable.Type, boundExpression.Type);

                if (boundOperator == null)
                {
                    Diagnostics.ReportUndefinedBinaryOperator(syntax.AssignmentToken.Location, syntax.AssignmentToken.Text, variable.Type, boundExpression.Type);
                    return new BoundErrorExpression(syntax);
                }

                var convertedExpression = BindConversion(syntax.Expression.Location, boundExpression, variable.Type);
                return new BoundCompoundAssignmentExpression(syntax, variable, boundOperator, convertedExpression);
            }
            else
            {
                var convertedExpression = BindConversion(syntax.Expression.Location, boundExpression, variable.Type);
                return new BoundAssignmentExpression(syntax, variable, convertedExpression);
            }
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);

            if (boundOperand.Type == TypeSymbol.Error)
            {
                return new BoundErrorExpression(syntax);
            }

            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

            if (boundOperator == null)
            {
                Diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundOperand.Type);
                return new BoundErrorExpression(syntax);
            }

            return new BoundUnaryExpression(syntax, boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);

            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
            {
                return new BoundErrorExpression(syntax);
            }

            if (syntax.OperatorToken.Kind.IsAssignmentOperator())
            {
                if (boundLeft is BoundVariableExpression variable)
                {
                    if (variable.Variable.IsReadOnly)
                    {
                        Diagnostics.ReportCannotAssign(syntax.OperatorToken.Location, variable.Variable.Name);
                    }

                    if (syntax.OperatorToken.Kind != SyntaxKind.EqualsToken)
                    {
                        var equivalentOperatorTokenKind = SyntaxFacts.GetBinaryOperatorOfAssignmentOperator(syntax.OperatorToken.Kind);
                        var boundAssignOperator = BoundBinaryOperator.Bind(equivalentOperatorTokenKind, variable.Type, boundRight.Type);

                        if (boundAssignOperator == null)
                        {
                            Diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, variable.Type, boundRight.Type);
                            return new BoundErrorExpression(syntax);
                        }

                        var convertedExpression = BindConversion(syntax.Right.Location, boundRight, variable.Type);
                        return new BoundCompoundAssignmentExpression(syntax, variable.Variable, boundAssignOperator, convertedExpression);
                    }
                    else
                    {
                        var convertedExpression = BindConversion(syntax.Right.Location, boundRight, variable.Type);
                        return new BoundAssignmentExpression(syntax, variable.Variable, convertedExpression);
                    }
                }

                if (boundLeft is BoundFieldAccessExpression field)
                {
                    if (field.StructMember.IsReadOnly)
                    {
                        Diagnostics.ReportCannotAssign(syntax.OperatorToken.Location, field.StructMember.Name);
                    }

                    if (syntax.OperatorToken.Kind != SyntaxKind.EqualsToken)
                    {
                        var equivalentOperatorTokenKind = SyntaxFacts.GetBinaryOperatorOfAssignmentOperator(syntax.OperatorToken.Kind);
                        var boundAssignOperator = BoundBinaryOperator.Bind(equivalentOperatorTokenKind, field.StructMember.Type, boundRight.Type);

                        if (boundAssignOperator == null)
                        {
                            Diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, field.StructMember.Type, boundRight.Type);
                            return new BoundErrorExpression(syntax);
                        }

                        var convertedExpression = BindConversion(syntax.Right.Location, boundRight, field.Type);
                        return new BoundCompoundFieldAssignmentExpression(syntax, field.StructInstance, field.StructMember, boundAssignOperator, convertedExpression);
                    }
                    else
                    {
                        var convertedExpression = BindConversion(syntax.Right.Location, boundRight, field.Type);
                        return new BoundFieldAssignmentExpression(syntax, field.StructInstance, field.StructMember, convertedExpression);
                    }
                }

                // TODO: @CLEANUP
                Diagnostics.ReportCannotAssign(syntax.OperatorToken.Location, "");
                return new BoundErrorExpression(syntax);
            }

            // Set up an implicit conversion if necessary
            if (boundLeft.Type != boundRight.Type)
            {
                var conversion = Conversion.Classify(boundRight.Type, boundLeft.Type);

                if (conversion.Exists && conversion.IsImplicit)
                {
                    boundRight = BindConversion(syntax.Right, boundLeft.Type, false);
                }
                else
                {
                    conversion = Conversion.Classify(boundLeft.Type, boundRight.Type);
                    if (conversion.Exists && conversion.IsImplicit)
                    {
                        boundLeft = BindConversion(syntax.Left, boundRight.Type, false);
                    }
                }
            }

            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator == null)
            {
                Diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression(syntax);
            }
            else if (boundOperator.Kind == BoundBinaryOperatorKind.Division && boundRight.ConstantValue != null && boundRight.ConstantValue.IsZero)
            {
                Diagnostics.ReportDivideByZero(syntax.Location);
                return new BoundErrorExpression(syntax);
            }

            return new BoundBinaryExpression(syntax, boundLeft, boundOperator, boundRight);
        }

        private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
        {
            // All built-in basic types have a conversion function with the same name that accepts one
            // parameter.
            var type = LookupType(syntax.Identifier.Text);

            if (syntax.Arguments.Count == 1 && type is not StructSymbol && type is TypeSymbol t)
            {
                return BindConversion(syntax.Arguments[0], t, allowExplicit: true);
            }

            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (var argument in syntax.Arguments)
            {
                var boundArgument = BindExpression(argument, false);
                boundArguments.Add(boundArgument);
            }

            var symbol = _scope.TryLookupSymbol(syntax.Identifier.Text);

            if (symbol == null)
            {
                Diagnostics.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Text);
                return new BoundErrorExpression(syntax);
            }

            if (symbol is StructSymbol)
            {
                symbol = _scope.TryLookupSymbol(syntax.Identifier.Text + ".ctor");
            }

            if (symbol is not FunctionSymbol)
            {
                Diagnostics.ReportNotAFunction(syntax.Identifier.Location, syntax.Identifier.Text);
                return new BoundErrorExpression(syntax);
            }

            var function = (FunctionSymbol) symbol;

            if (function.OverloadFor == null && syntax.Arguments.Count != function.Parameters.Length)
            {
                TextSpan span;
                if (syntax.Arguments.Count > function.Parameters.Length)
                {
                    SyntaxNode firstExceedingNode;
                    if (function.Parameters.Length > 0)
                    {
                        firstExceedingNode = syntax.Arguments.GetSeparator(function.Parameters.Length - 1);
                    }
                    else
                        firstExceedingNode = syntax.Arguments[0];
                    var lastExceedingArgument = syntax.Arguments[^1];
                    span = TextSpan.FromBounds(firstExceedingNode.Span.Start, lastExceedingArgument.Span.End);
                }
                else
                {
                    span = syntax.CloseParenthesisToken.Span;
                }

                var location = new TextLocation(syntax.SyntaxTree.Text, span);
                Diagnostics.ReportWrongArgumentCount(location, syntax.Identifier.Text, function.Parameters.Length, syntax.Arguments.Count);

                return new BoundErrorExpression(syntax);
            }
            else if (function.OverloadFor != null)
            {
                // Find best overload
                while (function != null)
                {
                    if (syntax.Arguments.Count != function.Parameters.Length || !MatchArgumentsAndParameters(boundArguments.ToImmutable(), function.Parameters))
                    {
                        function = function.OverloadFor;
                    }
                    else
                    {
                        break;
                    }
                }

                if (function == null)
                {
                    Diagnostics.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Text);
                    return new BoundErrorExpression(syntax);
                }
            }

            for (var i = 0; i < syntax.Arguments.Count; i++)
            {
                var argumentLocation = syntax.Arguments[i].Location;
                var argument = boundArguments[i];
                var parameter = function.Parameters[i];
                boundArguments[i] = BindConversion(argumentLocation, argument, parameter.Type);
            }

            if (syntax.FullyQualifiedIdentifier is MemberAccessExpressionSyntax @struct)
            {
                var instance = BindMemberAccessExpression(@struct);
                switch (instance)
                {
                    case BoundVariableExpression i:
                        return new BoundCallExpression(syntax, i, function, boundArguments.ToImmutable());
                    case BoundFieldAccessExpression i:
                        return new BoundCallExpression(syntax, i, function, boundArguments.ToImmutable());
                    case BoundThisExpression i:
                        return new BoundCallExpression(syntax, i, function, boundArguments.ToImmutable());
                    default:
                        return new BoundErrorExpression(syntax);
                }
            }
            else
            {
                return new BoundCallExpression(syntax, function, boundArguments.ToImmutable());
            }
        }

        private static bool MatchArgumentsAndParameters(ImmutableArray<BoundExpression> arguments, ImmutableArray<ParameterSymbol> parameters)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var expected = parameters[i];
                var conversion = Conversion.Classify(argument.Type, expected.Type);

                if (!conversion.Exists || conversion.IsExplicit)
                {
                    return false;
                }
            }

            return true;
        }

        private BoundExpression BindMemberAccessExpression(MemberAccessExpressionSyntax syntax)
        {
            if (syntax.Expression.Kind == SyntaxKind.NameExpression)
            {
                if (BindExpression(syntax.Expression) is not BoundVariableExpression expr)
                {
                    Diagnostics.ReportNotAClass(syntax.Expression.Location, syntax.Expression.ToString());
                    return new BoundErrorExpression(syntax);
                }

                var variable = BindFieldReference(expr, syntax.IdentifierToken);

                if (variable != null)
                {
                    return new BoundFieldAccessExpression(syntax, expr, variable);
                }

                var func = BindFunctionReference(syntax.IdentifierToken);

                if (func != null)
                {
                    return expr;
                }

                Diagnostics.ReportCannotAccessMember(syntax.IdentifierToken.Location, ((NameExpressionSyntax)syntax.Expression).IdentifierToken.Text);
                return new BoundErrorExpression(syntax);
            }
            else if (syntax.Expression.Kind == SyntaxKind.MemberAccessExpression)
            {
                if (BindExpression(syntax.Expression) is not BoundFieldAccessExpression expr)
                {
                    Diagnostics.ReportNotAClass(syntax.Expression.Location, syntax.Expression.ToString());
                    return new BoundErrorExpression(syntax);
                }

                var variable = BindFieldReference(expr, syntax.IdentifierToken);

                if (variable != null)
                {
                    return new BoundFieldAccessExpression(syntax, expr, variable);
                }

                var func = BindFunctionReference(syntax.IdentifierToken);

                if (func != null)
                {
                    return expr;
                }

                Diagnostics.ReportCannotAccessMember(syntax.IdentifierToken.Location, ((MemberAccessExpressionSyntax)syntax.Expression).IdentifierToken.Text);
                return new BoundErrorExpression(syntax);
            }
            else if (syntax.Expression.Kind == SyntaxKind.ThisKeyword)
            {
                if (_function == null)
                {
                    Diagnostics.ReportCannotUseThisOutsideOfAFunction(syntax.Expression.Location);
                    return new BoundErrorExpression(syntax);
                }

                if (_function.Receiver == null)
                {
                    Diagnostics.ReportCannotUseThisOutsideOfReceiverFunctions(syntax.Expression.Location, _function.Name);
                    return new BoundErrorExpression(syntax);
                }

                // Check if the struct has a member with the name of memberIdentifier
                VariableSymbol? variable = null;

                foreach (var member in _function.Receiver.Members)
                {
                    if (member.Name == syntax.IdentifierToken.Text)
                    {
                          variable = member;
                    }
                }

                if (variable != null)
                {
                    return new BoundFieldAccessExpression(syntax, new BoundThisExpression(syntax.Expression, _function.Receiver), variable);
                }

                var function = BindFunctionReference(syntax.IdentifierToken);

                if (function != null)
                {
                    return new BoundThisExpression(syntax.Expression, _function.Receiver);
                }

                Diagnostics.ReportUndefinedClassField(syntax.IdentifierToken.Location, syntax.IdentifierToken.Text);
                return new BoundErrorExpression(syntax);
            }
            else
            {
                Diagnostics.ReportCannotAccessMember(syntax.Expression.Location, syntax.Expression.ToString());
                return new BoundErrorExpression(syntax);
            }
        }

        private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = BindExpression(syntax);
            return BindConversion(syntax.Location, expression, type, allowExplicit);
        }

        private BoundExpression BindConversion(TextLocation diagnosticLocation, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                {
                    Diagnostics.ReportCannotConvert(diagnosticLocation, expression.Type, type);
                }

                return new BoundErrorExpression(expression.Syntax);
            }

            if (!allowExplicit && conversion.IsExplicit)
            {
                Diagnostics.ReportCannotConvertImplicitly(diagnosticLocation, expression.Type, type);
            }

            if (conversion.IsIdentity)
            {
                return expression;
            }

            return new BoundConversionExpression(expression.Syntax, type, expression);
        }

        private VariableSymbol BindVariableDeclaration(SyntaxToken identifier, bool isReadOnly, TypeSymbol type, BoundConstant? constant = null)
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;
            var variable = _function == null
                                ? (VariableSymbol)new GlobalVariableSymbol(name, isReadOnly, type, constant)
                                : new LocalVariableSymbol(name, isReadOnly, type, constant);

            if (declare && !_scope.TryDeclareVariable(variable))
            {
                Diagnostics.ReportSymbolAlreadyDeclared(identifier.Location, name);
            }

            return variable;
        }

        private VariableSymbol? BindFieldReference(BoundExpression variable, SyntaxToken memberIdentifier)
        {
            // Get type of variable, make sure it's a struct
            if (variable.Type.Kind != SymbolKind.Struct)
            {
                if (variable is BoundVariableExpression v)
                {
                    Diagnostics.ReportNotAClass(variable.Syntax.Location, v.Variable.Name);
                }
                else if (variable is BoundFieldAccessExpression f)
                {
                    Diagnostics.ReportNotAClass(variable.Syntax.Location, f.StructMember.Name);
                }
                else
                {
                    throw new Exception($"Unexpected expression type '{variable.Kind}'.");
                }

                return null;
            }

            // Check if the struct has a member with the name of memberIdentifier
            var type = (StructSymbol)variable.Type;

            foreach (var member in type.Members)
            {
                if (member.Name == memberIdentifier.Text)
                {
                    return member;
                }
            }

            return null;
        }

        private FunctionSymbol? BindFunctionReference(SyntaxToken identifierToken)
        {
            var name = identifierToken.Text;

            switch (_scope.TryLookupSymbol(name))
            {
                case FunctionSymbol func:
                    return  func;

                default:
                    Diagnostics.ReportNotAFunction(identifierToken.Location, name);
                    return null;
            }
        }

        private VariableSymbol? BindVariableReference(SyntaxToken identifierToken)
        {
            var name = identifierToken.Text;

            switch (_scope.TryLookupSymbol(name))
            {
                case VariableSymbol variable:
                    return variable;

                case null:
                    Diagnostics.ReportUndefinedVariable(identifierToken.Location, name);
                    return null;

                default:
                    Diagnostics.ReportNotAVariable(identifierToken.Location, name);
                    return null;
            }
        }

        private TypeSymbol? LookupType(string name)
        {
            switch (name)
            {
                case "object":
                    return TypeSymbol.Object;

                // Boolean types
                case "bool":
                    return TypeSymbol.Bool;

                // Integer types
                case "int8":
                    return TypeSymbol.Int8;

                case "int16":
                    return TypeSymbol.Int16;

                case "int32":
                    return TypeSymbol.Int32;

                case "int64":
                    return TypeSymbol.Int64;

                case "uint8":
                    return TypeSymbol.UInt8;

                case "uint16":
                    return TypeSymbol.UInt16;

                case "uint32":
                    return TypeSymbol.UInt32;

                case "uint64":
                    return TypeSymbol.UInt64;

                // Float types
                case "float32":
                    return TypeSymbol.Float32;

                case "float64":
                    return TypeSymbol.Float64;

                case "float128":
                    return TypeSymbol.Decimal;

                // String types
                case "char":
                    return TypeSymbol.Char;

                case "string":
                    return TypeSymbol.String;
                case "void":
                    return TypeSymbol.Void;

                default:
                    var maybeSymbol = _scope.TryLookupSymbol(name);

                    if (maybeSymbol is TypeSymbol s)
                    {
                        return s;
                    }

                    return null;
            }
        }
    }
}