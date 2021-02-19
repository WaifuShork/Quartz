using System.Collections.Generic;
using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class VariableDeclarationSyntax : StatementSyntax
    {
        public VariableDeclarationSyntax(SyntaxTree syntaxTree, 
                                         TypeClauseSyntax type, 
                                         SyntaxToken identifier, 
                                         SyntaxToken equalsToken, 
                                         ExpressionSyntax initializer) 
                                         : base(syntaxTree)
        {
            Type = type;
            Identifier = identifier;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }
        
        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;

        public TypeClauseSyntax Type { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Initializer { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Type;
            yield return Identifier;
            yield return EqualsToken;
            yield return Initializer;
        }
    }
}