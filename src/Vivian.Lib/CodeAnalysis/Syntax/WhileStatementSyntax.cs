namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class WhileStatementSyntax : StatementSyntax
    {
        public WhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken whileKeyword, SyntaxToken openParenthesisToken, ExpressionSyntax condition, SyntaxToken closeParenthesisToken, StatementSyntax body) : base(syntaxTree)
        {
            WhileKeyword = whileKeyword;
            OpenParenthesisToken = openParenthesisToken;
            Condition = condition;
            CloseParenthesisToken = closeParenthesisToken;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        
        public SyntaxToken WhileKeyword { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax Condition { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public StatementSyntax Body { get; }
    }
}