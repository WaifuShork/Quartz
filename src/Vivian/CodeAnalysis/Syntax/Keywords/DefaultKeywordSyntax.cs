using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    internal sealed class DefaultKeywordSyntax : ExpressionSyntax
    {
        internal DefaultKeywordSyntax(SyntaxTree syntaxTree, SyntaxToken keyword)
            : base(syntaxTree)
        {
            Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.DefaultKeyword;
        
        public SyntaxToken Keyword { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
        }
    }
}