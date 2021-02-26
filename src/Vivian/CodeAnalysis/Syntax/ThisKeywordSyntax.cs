using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    internal sealed partial class ThisKeywordSyntax : ExpressionSyntax
    {
        internal ThisKeywordSyntax(SyntaxTree syntaxTree, SyntaxToken keyword)
            : base(syntaxTree)
        {
            Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.ThisKeyword;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
        }

        public SyntaxToken Keyword { get; }
    }
}