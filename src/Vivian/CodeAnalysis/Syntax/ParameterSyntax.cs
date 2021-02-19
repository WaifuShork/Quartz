using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax(SyntaxTree syntaxTree, TypeClauseSyntax type, SyntaxToken identifier) : base(syntaxTree)
        {
            Type = type;
            Identifier = identifier;
        }
        
        public override SyntaxKind Kind => SyntaxKind.Parameter;
        
        public TypeClauseSyntax Type { get; }
        public SyntaxToken Identifier { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return Type;
        }
    }
}