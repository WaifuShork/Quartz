using System.Collections.Generic;
using Vivian.CodeAnalysis.Binding;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class FunctionDeclarationSyntax : MemberSyntax
    {
        public FunctionDeclarationSyntax(SyntaxTree syntaxTree, 
                                         TypeClauseSyntax type, 
                                         SyntaxToken identifier,
                                         SyntaxToken openParenthesisToken, 
                                         SeparatedSyntaxList<ParameterSyntax> parameters,
                                         SyntaxToken closedParenthesisToken, 
                                         BlockStatementSyntax body) 
                                         : base(syntaxTree)
        {
            Type = type;
            Identifier = identifier;
            OpenParenthesisToken = openParenthesisToken;
            Parameters = parameters;
            CloseParenthesisToken = closedParenthesisToken;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;
        
        public TypeClauseSyntax Type { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public BlockStatementSyntax Body { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Type;
            yield return Identifier;
            yield return OpenParenthesisToken;
            yield return CloseParenthesisToken;
            yield return Type!;
            yield return Body;
        }
    }
}