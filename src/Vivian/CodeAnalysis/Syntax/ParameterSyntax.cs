using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class ParameterSyntax : SyntaxNode
    {
        internal ParameterSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, TypeClauseSyntax type)
            : base(syntaxTree)
        {
            Identifier = identifier;
            Type = type;
        }

        public override SyntaxKind Kind => SyntaxKind.Parameter;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return Type;
        }

        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax Type { get; }
    }
}