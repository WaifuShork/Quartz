namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class ReturnStatementSyntax : StatementSyntax
    {
        public ReturnStatementSyntax(SyntaxTree syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken) : base(syntaxTree)
        {
            ReturnKeyword = returnKeyword;
            Expression = expression;
            SemicolonToken = semicolonToken;
        }

        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
        
        public SyntaxToken ReturnKeyword { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken SemicolonToken { get; }
    }
}