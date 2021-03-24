using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;
using Vivian.CodeAnalysis.Text;

namespace Vivian.CodeAnalysis
{
    // VE - Vivian Error
    // VE - Vivian Warning
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(IEnumerable<Diagnostic> diagnostics)
        {
            _diagnostics.AddRange(diagnostics);
        }

        private void ReportError(TextLocation location, string message)
        {
            var diagnostic = Diagnostic.Error(location, message);
            _diagnostics.Add(diagnostic);
        }

        // TODO: Emit warnings for IDE debugging/editor warnings
        private void ReportWarning(TextLocation location, string message)
        {
            var diagnostic = Diagnostic.Warning(location, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportInternalCompilerError(TextLocation location)
        {
            const string? message = "VE0001: (Internal compiler error) - Try to determine whether the compiler is failing because of its inability to parse unexpected syntax. If you receive this error repeatedly, please contact developers.";
            ReportError(location, message);
        }

        internal void ReportDivideByZero(TextLocation location)
        {
            const string? message = "VE0002: Division by zero.";
            ReportError(location, message);
        }

        public void ReportInvalidNumber(TextLocation location, string text, TypeSymbol type)
        {
            var message = $"VE0003: The number '{text}' isn't valid <{type}>.";
            ReportError(location, message);
        }

        public void ReportBadCharacter(TextLocation location, char character)
        {
            var message = $"VE0004: Bad character input: '{character}'.";
            ReportError(location, message);
        }

        internal void ReportEmptyCharConst(TextLocation location)
        {
            const string? message = "VE0004: Empty character constant.";
            ReportError(location, message);
        }

        internal void ReportInvalidCharConst(TextLocation location)
        {
            const string? message = "VE0005: Character constant must be a single character surrounded by single quotes.";
            ReportError(location, message);
        }

        public void ReportUnterminatedString(TextLocation location)
        {
            const string? message = "VE0006: Unterminated string literal.";
            ReportError(location, message);
        }

        public void ReportUnterminatedMultiLineComment(TextLocation location)
        {
            const string? message = "VE0007: Unterminated multi-line comment.";
            ReportError(location, message);
        }

        public void ReportUnexpectedToken(TextLocation location, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"VE0008: Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            ReportError(location, message);
        }

        public void ReportUnexpectedToken(TextLocation location, SyntaxKind actualKind, SyntaxKind expectedKind1, SyntaxKind expectedKind2)
        {
            var message = $"VE0009: Unexpected token <{actualKind}>, expected <{expectedKind1}> or <{expectedKind2}>.";
            ReportError(location, message);
        }

        public void ReportUndefinedUnaryOperator(TextLocation location, string operatorText, TypeSymbol operandType)
        {
            var message = $"VE0010: Unary operator '{operatorText}' is not defined for type <{operandType}>.";
            ReportError(location, message);
        }

        public void ReportUndefinedBinaryOperator(TextLocation location, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            var message = $"VE0011: Binary operator '{operatorText}' is not defined for types <{leftType}> and <{rightType}>.";
            ReportError(location, message);
        }

        public void ReportParameterAlreadyDeclared(TextLocation location, string parameterName)
        {
            var message = $"VE0012: A parameter with the name '{parameterName}' already exists.";
            ReportError(location, message);
        }

        public void ReportUndefinedClassField(TextLocation location, string name)
        {
            var message = $"VE0013: Class field '{name}' doesn't exist.";
            ReportError(location, message);
        }

        public void ReportUndefinedVariable(TextLocation location, string name)
        {
            var message = $"VE0014: Variable '{name}' doesn't exist.";
            ReportError(location, message);
        }

        public void ReportNotAVariable(TextLocation location, string name)
        {
            var message = $"VE0015: '{name}' is not a variable.";
            ReportError(location, message);
        }

        public void ReportNotAClass(TextLocation location, string name)
        {
            var message = $"VE0016: '{name}' is not a valid class.";
            ReportError(location, message);
        }

        public void ReportUndefinedType(TextLocation location, string name)
        {
            var message = $"VE0017: Type '{name}' doesn't exist.";
            ReportError(location, message);
        }

        public void ReportCannotConvert(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"VE0018: Cannot convert type <{fromType}> to <{toType}>.";
            ReportError(location, message);
        }

        public void ReportCannotConvertImplicitly(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"VE0019: Cannot convert type <{fromType}> to <{toType}>. An explicit conversion exists (are you missing a cast?)";
            ReportError(location, message);
        }

        public void ReportSymbolAlreadyDeclared(TextLocation location, string name)
        {
            var message = $"VE0020: '{name}' is already declared.";
            ReportError(location, message);
        }

        public void ReportCannotAssign(TextLocation location, string name)
        {
            var message = $"VE0021: Variable '{name}' is read-only and cannot be assigned to.";
            ReportError(location, message);
        }

        public void ReportUndefinedFunction(TextLocation location, string name)
        {
            var message = $"VE0022: Function '{name}' doesn't exist.";
            ReportError(location, message);
        }

        public void ReportNotAFunction(TextLocation location, string name)
        {
            var message = $"VE0023: '{name}' is not a function.";
            ReportError(location, message);
        }
        
        internal void ReportCannotUseThisOutsideOfReceiverFunctions(TextLocation location, string name)
        {
            var message = $"VE0024: This can only be used in functions with a class receiver. Function '{name}' has no receiver defined.";
            ReportError(location, message);
        }

        internal void ReportCannotUseThisOutsideOfAFunction(TextLocation location)
        {
            const string? message = "VE0025: This can only by used in functions with a class receiver.";
            ReportError(location, message);
        }

        public void ReportWrongArgumentCount(TextLocation location, string name, int expectedCount, int actualCount)
        {
            var message = $"VE0026: Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
            ReportError(location, message);
        }

        public void ReportExpressionMustHaveValue(TextLocation location)
        {
            const string? message = "VE0027: Expression must have a value.";
            ReportError(location, message);
        }

        public void ReportInvalidBreakOrContinue(TextLocation location, string text)
        {
            var message = $"VE0028: The keyword '{text}' can only be used inside of loops.";
            ReportError(location, message);
        }

        public void ReportAllPathsMustReturn(TextLocation location)
        {
            const string? message = "VE0029: Not all code paths return a value.";
            ReportError(location, message);
        }

        public void ReportInvalidReturnExpression(TextLocation location, string functionName)
        {
            var message = $"VE0030: Since the function '{functionName}' does not return a value the 'return' keyword cannot be followed by an expression.";
            ReportError(location, message);
        }

        public void ReportInvalidReturnWithValueInGlobalStatements(TextLocation location)
        {
            const string? message = "VE0031: The 'return' keyword cannot be followed by an expression in global statements.";
            ReportError(location, message);
        }

        public void ReportMissingReturnExpression(TextLocation location, TypeSymbol returnType)
        {
            var message = $"VE0032: An expression of type <{returnType}> is expected.";
            ReportError(location, message);
        }

        public void ReportInvalidExpressionStatement(TextLocation location)
        {
            const string? message = "VE0033: Only assignment and call expressions can be used as a statement.";
            ReportError(location, message);
        }

        public void ReportInvalidAssignmentExpressionStatement(TextLocation location)
        {
            const string? message = "VE0034: Only field declarations and assignment expressions are allowed in classes.";
            ReportError(location, message);
        }

        public void ReportOnlyOneFileCanHaveGlobalStatements(TextLocation location)
        {
            const string? message = "VE0035: At most one file can have global statements.";
            ReportError(location, message);
        }

        public void ReportMainMustHaveCorrectSignature(TextLocation location)
        {
            const string? message = "VE0036: Main must not take arguments and not return anything.";
            ReportError(location, message);
        }

        public void ReportCannotMixMainAndGlobalStatements(TextLocation location)
        {
            const string? message = "VE0037: Cannot declare Main function when global statements are used.";
            ReportError(location, message);
        }

        public void ReportInvalidReference(string path)
        {
            var message = $"VE0038: The reference is not a valid .NET assembly: '{path}'.";
            ReportError(default, message);
        }

        internal void ReportCannotAccessMember(TextLocation location, string text)
        {
            var message = $"VE0039: Cannot access members of '{text}'. Only members of classes can be accessed using the '.' operator.";
            ReportError(location, message);
        }

        public void ReportRequiredTypeNotFound(string? vivianName, string metadataName)
        {
            var message = vivianName == null
                ? $"VE0040: The required type '{metadataName}' cannot be resolved among the given references."
                : $"VE0041: The required type '{vivianName}' ('{metadataName}') cannot be resolved among the given references.";
            ReportError(default, message);
        }

        public void ReportRequiredTypeAmbiguous(string? vivianName, string metadataName, TypeDefinition[] foundTypes)
        {
            var assemblyNames = foundTypes.Select(t => t.Module.Assembly.Name.Name);
            var assemblyNameList = string.Join(", ", assemblyNames);
            var message = vivianName == null
                ? $"VE0042: The required type '{metadataName}' was found in multiple references: {assemblyNameList}."
                : $"VE0043: The required type '{vivianName}' ('{metadataName}') was found in multiple references: {assemblyNameList}.";
            ReportError(default, message);
        }

        public void ReportRequiredMethodNotFound(string typeName, string methodName, string[] parameterTypeNames)
        {
            var parameterTypeNameList = string.Join(", ", parameterTypeNames);
            var message = $"VE0044: The required method '{typeName}.{methodName}({parameterTypeNameList})' cannot be resolved among the given references.";
            ReportError(default, message);
        }

        private void ReportUnreachableCode(TextLocation location)
        {
            const string? message = "VE0045: Unreachable code detected.";
            ReportWarning(location, message);
        }

        public void ReportUnreachableCode(SyntaxNode node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.BlockStatement:
                    var firstStatement = ((BlockStatementSyntax)node).Statements.FirstOrDefault();
                    // Report just for non empty blocks.
                    if (firstStatement != null)
                    {
                        ReportUnreachableCode(firstStatement);
                    }
                    return;
                case SyntaxKind.VariableDeclaration:
                    ReportUnreachableCode(((VariableDeclarationSyntax)node).Keyword.Location);
                    return;
                case SyntaxKind.IfStatement:
                    ReportUnreachableCode(((IfStatementSyntax)node).IfKeyword.Location);
                    return;
                case SyntaxKind.WhileStatement:
                    ReportUnreachableCode(((WhileStatementSyntax)node).WhileKeyword.Location);
                    return;
                case SyntaxKind.DoWhileStatement:
                    ReportUnreachableCode(((DoWhileStatementSyntax)node).DoKeyword.Location);
                    return;
                case SyntaxKind.ForStatement:
                    ReportUnreachableCode(((ForStatementSyntax)node).Keyword.Location);
                    return;
                case SyntaxKind.BreakStatement:
                    ReportUnreachableCode(((BreakStatementSyntax)node).Keyword.Location);
                    return;
                case SyntaxKind.ContinueStatement:
                    ReportUnreachableCode(((ContinueStatementSyntax)node).Keyword.Location);
                    return;
                case SyntaxKind.ReturnStatement:
                    ReportUnreachableCode(((ReturnStatementSyntax)node).ReturnKeyword.Location);
                    return;
                case SyntaxKind.ExpressionStatement:
                    var expression = ((ExpressionStatementSyntax)node).Expression;
                    ReportUnreachableCode(expression);
                    return;
                case SyntaxKind.CallExpression:
                    ReportUnreachableCode(((CallExpressionSyntax)node).Identifier.Location);
                    return;
                default:
                    throw new Exception($"Unexpected syntax {node.Kind}");
            }
        }
    }
}
