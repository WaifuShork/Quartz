using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Syntax
{
    partial class AssignmentExpressionSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
            yield return EqualsToken;
            yield return Expression;
        }
    }
    partial class BinaryExpressionSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }
    partial class BlockStatementSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBraceToken;
            yield return CloseBraceToken;
        }
    }
    partial class BreakStatementSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
            yield return SemicolonToken;
        }
    }
    partial class CallExpressionSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return OpenParenthesisToken;
            yield return CloseParenthesisToken;
        }
    }
    partial class CompilationUnitSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return EndOfFileToken;
        }
    }
    partial class ContinueStatementSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
            yield return SemicolonToken;
        }
    }
    partial class DoWhileStatementSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return DoKeyword;
            yield return Body;
            yield return WhileKeyword;
            yield return OpenParenthesisToken;
            yield return Condition;
            yield return CloseParenthesisToken;
            yield return SemicolonToken;
        }
    }
    partial class ElseClauseSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ElseKeyword;
            yield return ElseStatement;
        }
    }
    partial class ExpressionStatementSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
        }
    }
    partial class ForStatementSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
            yield return OpenParenthesisToken;
            yield return Identifier;
            yield return EqualsToken;
            yield return LowerBound;
            yield return ToKeyword;
            yield return UpperBound;
            yield return CloseParenthesisToken;
            yield return Body;
        }
    }
    partial class FunctionDeclarationSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return FunctionKeyword;
            yield return Identifier;
            yield return OpenParenthesisToken;
            yield return CloseParenthesisToken;
            yield return Type;
            yield return Body;
        }
    }
    partial class GlobalStatementSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Statement;
        }
    }
    partial class IfStatementSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IfKeyword;
            yield return OpenParenthesisToken;
            yield return Condition;
            yield return CloseParenthesisToken;
            yield return ThenStatement;
            yield return ElseClause;
        }
    }
    partial class LiteralExpressionSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LiteralToken;
        }
    }
    partial class NameExpressionSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
        }
    }
    partial class ParameterSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return Type;
        }
    }
    partial class ParenthesizedExpressionSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return CloseParenthesisToken;
        }
    }
    partial class ReturnStatementSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ReturnKeyword;
            yield return Expression;
            yield return SemicolonToken;
        }
    }
    partial class TypeClauseSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ColonToken;
            yield return Identifier;
        }
    }
    partial class UnaryExpressionSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OperatorToken;
            yield return Operand;
        }
    }
    partial class VariableDeclarationSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
            yield return Identifier;
            yield return TypeClause;
            yield return EqualsToken;
            yield return Initializer;
        }
    }
    partial class WhileStatementSyntax
    {
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return WhileKeyword;
            yield return OpenParenthesisToken;
            yield return Condition;
            yield return CloseParenthesisToken;
            yield return Body;
        }
    }
}