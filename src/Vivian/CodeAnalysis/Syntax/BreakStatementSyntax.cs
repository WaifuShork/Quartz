using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    internal sealed partial class BreakStatementSyntax : StatementSyntax
    {
        internal BreakStatementSyntax(SyntaxTree syntaxTree, SyntaxToken keyword)
            : base(syntaxTree)
        {
            Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.BreakStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
        }

        public SyntaxToken Keyword { get; }
    }
}