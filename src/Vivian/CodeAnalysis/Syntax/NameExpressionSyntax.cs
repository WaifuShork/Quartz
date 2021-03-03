using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class NameExpressionSyntax : ExpressionSyntax
    {
        internal NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken)
            : base(syntaxTree)
        {
            IdentifierToken = identifierToken;
        }

        public override SyntaxKind Kind => SyntaxKind.NameExpression;
        
        public SyntaxToken IdentifierToken { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
        }
    }
}