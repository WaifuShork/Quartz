using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    internal sealed class BreakStatementSyntax : StatementSyntax
    {
        internal BreakStatementSyntax(SyntaxTree syntaxTree, SyntaxToken keyword)
            : base(syntaxTree)
        {
            Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.BreakStatement;
        
        public SyntaxToken Keyword { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
        }
    }
}