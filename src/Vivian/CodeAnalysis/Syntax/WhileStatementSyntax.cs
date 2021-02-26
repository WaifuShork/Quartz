using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class WhileStatementSyntax : StatementSyntax
    {
        internal WhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken whileKeyword, ExpressionSyntax condition, StatementSyntax body)
            : base(syntaxTree)
        {
            WhileKeyword = whileKeyword;
            Condition = condition;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return WhileKeyword;
            yield return Condition;
            yield return Body;
        }

        public SyntaxToken WhileKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }
    }
}