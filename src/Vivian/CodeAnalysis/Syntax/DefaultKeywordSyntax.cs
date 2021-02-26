using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    internal sealed partial class DefaultKeywordSyntax : ExpressionSyntax
    {
        internal DefaultKeywordSyntax(SyntaxTree syntaxTree, SyntaxToken keyword)
            : base(syntaxTree)
        {
            Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.DefaultKeyword;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
        }

        public SyntaxToken Keyword { get; }
    }
}