using System.Collections.Generic;
using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class BlockStatementSyntax : StatementSyntax
    {
        internal BlockStatementSyntax(SyntaxTree syntaxTree, SyntaxToken openBraceToken, ImmutableArray<StatementSyntax> statements, SyntaxToken closeBraceToken)
            : base(syntaxTree)
        {
            OpenBraceToken = openBraceToken;
            Statements = statements;
            CloseBraceToken = closeBraceToken;
        }

        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBraceToken;
            yield return CloseBraceToken;
        }

        public SyntaxToken OpenBraceToken { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseBraceToken { get; }
    }
}