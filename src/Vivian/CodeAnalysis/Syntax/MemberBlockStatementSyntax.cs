using System.Collections.Generic;
using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class MemberBlockStatementSyntax : MemberSyntax
    {
        internal MemberBlockStatementSyntax(SyntaxTree syntaxTree, SyntaxToken openBrace, ImmutableArray<StatementSyntax> statements, SyntaxToken closeBrace)
            : base(syntaxTree)
        {
            OpenBrace = openBrace;
            Statement = statements;
            CloseBrace = closeBrace;
        }

        public override SyntaxKind Kind => SyntaxKind.MemberBlockStatement;
        
        public SyntaxToken OpenBrace { get; }
        public ImmutableArray<StatementSyntax> Statement { get; }
        public SyntaxToken CloseBrace { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBrace;
            yield return CloseBrace;
        }
    }
}