using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class WhileStatementSyntax : StatementSyntax
    {
        internal WhileStatementSyntax(SyntaxTree syntaxTree, 
                                      SyntaxToken whileKeyword, 
                                      SyntaxToken openParenthesisToken,
                                      ExpressionSyntax condition, 
                                      SyntaxToken closeParenthesisToken,
                                      StatementSyntax body)
                                      : base(syntaxTree)
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
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return WhileKeyword;
            yield return OpenParenthesisToken;
            yield return Condition;
            yield return CloseParenthesisToken;
            yield return Body;
        }
    }
}