using System.Collections.Generic;
using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class VariableDeclarationSyntax : StatementSyntax
    {
        internal VariableDeclarationSyntax(SyntaxTree syntaxTree, SyntaxToken keyword, SyntaxToken identifier, TypeClauseSyntax? typeClause, SyntaxToken? equalsToken, ExpressionSyntax? initializer)
            : base(syntaxTree)
        {
            Keyword = keyword;
            Identifier = identifier;
            TypeClause = typeClause;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
            yield return Identifier;
            yield return TypeClause;
            yield return EqualsToken;
            yield return Initializer;
        }

        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax? TypeClause { get; }
        public SyntaxToken? EqualsToken { get; }
        public ExpressionSyntax? Initializer { get; }
    }
}