using System.Collections.Generic;
using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class CompilationUnitSyntax : SyntaxNode
    {
        internal CompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken)
            : base(syntaxTree)
        {
            Members = members;
            EndOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return EndOfFileToken;
        }

        public ImmutableArray<MemberSyntax> Members { get; }
        public SyntaxToken EndOfFileToken { get; }
    }
}