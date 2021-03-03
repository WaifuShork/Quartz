using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        internal LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken)
            : this(syntaxTree, literalToken, literalToken.Value!) { }

        internal LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken, object value)
            : base(syntaxTree)
        {
            LiteralToken = literalToken;
            Value = value;
        }

        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
        
        public SyntaxToken LiteralToken { get; }
        public object Value { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LiteralToken;
        }
    }
}