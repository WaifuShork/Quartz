using wsc.CodeAnalysis.Syntax;

namespace wsc.CodeAnalysis.Binding
{
    public sealed class CallExpressionSyntax : ExpressionSyntax
    {
        public CallExpressionSyntax(SyntaxToken identifier, SyntaxToken openParenthesisToken, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closeParenthesis)
        {
            Identifier = identifier;
            OpenParenthesisToken = openParenthesisToken;
            Arguments = arguments;
            CloseParenthesis = closeParenthesis;
        }
        
        public override SyntaxKind Kind => SyntaxKind.CallExpression;
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
        public SyntaxToken CloseParenthesis { get; }
    }
}