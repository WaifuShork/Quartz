namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class IfStatementSyntax : StatementSyntax
    {
        public IfStatementSyntax(SyntaxTree syntaxTree, SyntaxToken ifKeyword, SyntaxToken openParenthesisToken, ExpressionSyntax condition, SyntaxToken closeParenthesisToken, StatementSyntax thenStatement, ElseClauseSyntax elseClause) : base(syntaxTree)
        {
            IfKeyword = ifKeyword;
            OpenParenthesisToken = openParenthesisToken;
            Condition = condition;
            CloseParenthesisToken = closeParenthesisToken;
            ThenStatement = thenStatement;
            ElseClause = elseClause;
        }
        
        public override SyntaxKind Kind => SyntaxKind.IfStatement;
        public SyntaxToken IfKeyword { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax Condition { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseClauseSyntax ElseClause { get; }
    }
}