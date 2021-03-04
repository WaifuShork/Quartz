using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class ParameterSyntax : SyntaxNode
    {
        internal ParameterSyntax(SyntaxTree syntaxTree,  SyntaxToken identifier, TypeClauseSyntax type)
            : base(syntaxTree)
        {
            Identifier = identifier;
            Type = type;
        }

        public override SyntaxKind Kind => SyntaxKind.Parameter;
        
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax Type { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Type;
            yield return Identifier;
        }
    }
}