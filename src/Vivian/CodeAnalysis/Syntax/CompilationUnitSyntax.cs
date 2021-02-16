using System.Collections.Generic;
using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken) : base(syntaxTree)
        {
            Members = members;
            EndOfFileToken = endOfFileToken;
        }
        
        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public ImmutableArray<MemberSyntax> Members { get; }
        public SyntaxToken EndOfFileToken { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return EndOfFileToken;
        }
    }
}