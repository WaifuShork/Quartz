namespace Vivian.CodeAnalysis.Syntax
{
    public class ExpressionStatementSyntax : StatementSyntax
    {
        public ExpressionStatementSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression) : base(syntaxTree)
        {
            Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        
        public ExpressionSyntax Expression { get; }

    }
}