using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class MemberAccessExpressionSyntax : ExpressionSyntax
    {
        internal MemberAccessExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken operatorToken, SyntaxToken identifierToken)
            : base(syntaxTree)
        {
            IdentifierToken = identifierToken;
            OperatorToken = operatorToken;
            Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.MemberAccessExpression;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
            yield return OperatorToken;
            yield return Expression;
        }

        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Expression { get; }

    }
}