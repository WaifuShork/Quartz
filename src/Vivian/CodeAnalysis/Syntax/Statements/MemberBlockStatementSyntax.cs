using System.Collections.Generic;
using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class MemberBlockStatementSyntax : MemberSyntax
    {
        internal MemberBlockStatementSyntax(SyntaxTree syntaxTree, SyntaxToken openBrace, ImmutableArray<StatementSyntax> statements, ImmutableArray<MemberSyntax> members, SyntaxToken closeBrace)
            : base(syntaxTree)
        {
            OpenBrace = openBrace;
            Statements = statements;
            Members = members;
            CloseBrace = closeBrace;
        }

        public override SyntaxKind Kind => SyntaxKind.MemberBlockStatement;
        
        public SyntaxToken OpenBrace { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public ImmutableArray<MemberSyntax> Members { get; }
        public SyntaxToken CloseBrace { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBrace;
            yield return CloseBrace;
        }
    }
}