using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    internal sealed class ThisKeywordSyntax : ExpressionSyntax
    {
        internal ThisKeywordSyntax(SyntaxTree syntaxTree, SyntaxToken keyword)
            : base(syntaxTree)
        {
            Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.ThisKeyword;
        
        public SyntaxToken Keyword { get; }
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
        }
    }
}