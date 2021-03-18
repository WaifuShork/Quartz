using System.Collections.Generic;
using Vivian.CodeAnalysis.Binding;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class FunctionDeclarationSyntax : MemberSyntax
    {
        internal FunctionDeclarationSyntax(SyntaxTree syntaxTree, 
                                           SyntaxToken functionKeyword,
                                           SyntaxToken? receiver,
                                           SyntaxToken? dotToken,
                                           SyntaxToken identifier,
                                           SyntaxToken openParenthesisToken,
                                           SeparatedSyntaxList<ParameterSyntax> parameters,
                                           SyntaxToken closeParenthesisToken,
                                           TypeClauseSyntax type,
                                           BlockStatementSyntax body) 
                                           : base(syntaxTree)
        {
            FunctionKeyword = functionKeyword;
            Receiver = receiver;
            DotToken = dotToken;
            Identifier = identifier;
            OpenParenthesisToken = openParenthesisToken;
            Parameters = parameters;
            CloseParenthesisToken = closeParenthesisToken;
            Type = type;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;

        public SyntaxToken FunctionKeyword { get; }
        public SyntaxToken? Receiver { get; }
        public SyntaxToken? DotToken { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public TypeClauseSyntax Type { get; }
        public BlockStatementSyntax Body { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return FunctionKeyword;
            yield return Receiver!;
            yield return DotToken!;
            yield return Identifier;
            yield return OpenParenthesisToken;
            yield return CloseParenthesisToken;
            yield return Type;
            yield return Body;
        }
    }
}