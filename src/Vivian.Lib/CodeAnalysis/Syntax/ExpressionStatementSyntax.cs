namespace wsc.CodeAnalysis.Syntax
{
    public class ExpressionStatementSyntax : StatementSyntax
    {

        public ExpressionStatementSyntax(ExpressionSyntax expression)
        {
            Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        
        public ExpressionSyntax Expression { get; }

    }
}