namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundDoWhileStatement : BoundLoopStatement
    {
        public BoundDoWhileStatement(BoundStatement body, BoundExpression condition, BoundLabel breakLabel, BoundLabel continueLabel) : base(breakLabel, continueLabel)
        {
            Body = body;
            Condition = condition;
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;
        public BoundStatement Body { get; }
        public BoundExpression Condition { get; }
        public new BoundLabel BreakLabel { get; }
        public new BoundLabel ContinueLabel { get; }
    }
}