using System.Collections.Generic;
using Vivian.CodeAnalysis.Binding;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class DoWhileStatementSyntax : StatementSyntax
    {
        internal DoWhileStatementSyntax(SyntaxTree syntaxTree, 
                                        SyntaxToken doKeyword, 
                                        StatementSyntax body, 
                                        SyntaxToken whileKeyword,
                                        SyntaxToken openParenthesisToken,
                                        ExpressionSyntax condition,
                                        SyntaxToken closeParenthesisToken)
                                        : base(syntaxTree)
        {
            DoKeyword = doKeyword;
            Body = body;
            WhileKeyword = whileKeyword;
            OpenParenthesisToken = openParenthesisToken;
            Condition = condition;
            CloseParenthesisToken = closeParenthesisToken;
        }

        public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;
        
        public SyntaxToken DoKeyword { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken WhileKeyword { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax Condition { get; }
        public SyntaxToken CloseParenthesisToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return DoKeyword;
            yield return Body;
            yield return WhileKeyword;
            yield return OpenParenthesisToken;
            yield return Condition;
            yield return CloseParenthesisToken;
        }
    }
}