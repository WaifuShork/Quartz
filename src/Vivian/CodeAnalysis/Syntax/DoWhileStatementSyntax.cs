using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class DoWhileStatementSyntax : StatementSyntax
    {
        public DoWhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken doKeyword, StatementSyntax body, SyntaxToken whileKeyword, SyntaxToken openParenthesisToken, ExpressionSyntax condition, SyntaxToken closeParenthesisToken, SyntaxToken semicolonToken) : base(syntaxTree)
        {
            DoKeyword = doKeyword;
            Body = body;
            WhileKeyword = whileKeyword;
            OpenParenthesisToken = openParenthesisToken;
            Condition = condition;
            CloseParenthesisToken = closeParenthesisToken;
            SemicolonToken = semicolonToken;
        }

        public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;
        public SyntaxToken DoKeyword { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken WhileKeyword { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax Condition { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public SyntaxToken SemicolonToken { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return DoKeyword;
            yield return Body;
            yield return WhileKeyword;
            yield return OpenParenthesisToken;
            yield return Condition;
            yield return CloseParenthesisToken;
            yield return SemicolonToken;
        }
    }
}