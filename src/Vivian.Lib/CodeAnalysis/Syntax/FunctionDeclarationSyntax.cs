using Vivian.CodeAnalysis.Binding;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class FunctionDeclarationSyntax : MemberSyntax
    {
        public FunctionDeclarationSyntax(SyntaxToken functionKeyword, 
                                        SyntaxToken identifier,
                                        SyntaxToken openParenthesisToken, 
                                        SeparatedSyntaxList<ParameterSyntax> parameters,
                                        SyntaxToken closedParenthesisToken, 
                                        TypeClauseSyntax type, 
                                        BlockStatementSyntax body)
        {
            FunctionKeyword = functionKeyword;
            Identifier = identifier;
            OpenParenthesisToken = openParenthesisToken;
            Parameters = parameters;
            ClosedParenthesisToken = closedParenthesisToken;
            Type = type;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;
        public SyntaxToken FunctionKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken ClosedParenthesisToken { get; }
        public TypeClauseSyntax Type { get; }
        public BlockStatementSyntax Body { get; }
    }
}