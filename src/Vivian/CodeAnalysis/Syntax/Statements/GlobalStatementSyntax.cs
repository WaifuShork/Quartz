using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class GlobalStatementSyntax : MemberSyntax
    {
        internal GlobalStatementSyntax(SyntaxTree syntaxTree, StatementSyntax statement)
            : base(syntaxTree)
        {
            Statement = statement;
        }

        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
        
        public StatementSyntax Statement { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Statement;
        }
    }
}