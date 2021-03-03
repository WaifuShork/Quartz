using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    internal sealed class ContinueStatementSyntax : StatementSyntax
    {
        internal ContinueStatementSyntax(SyntaxTree syntaxTree, SyntaxToken keyword)
            : base(syntaxTree)
        {
            Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
        
        public SyntaxToken Keyword { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
        }
    }
}