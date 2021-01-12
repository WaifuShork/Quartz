namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundWhileStatement : BoundLoopStatement
    {
        public BoundWhileStatement(BoundExpression condition, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel) : base(breakLabel, continueLabel)
        {
            Condition = condition;
            Body = body;
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}