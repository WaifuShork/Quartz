namespace wsc.CodeAnalysis.Binding
{
    internal sealed class BoundConditionalGotoStatement : BoundStatement
    {

        public BoundConditionalGotoStatement(BoundLabel boundLabel, BoundExpression condition, bool jumpIfTrue = true)
        {
            BoundLabel = boundLabel;
            Condition = condition;
            JumpIfTrue = jumpIfTrue;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
        public BoundLabel BoundLabel { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfTrue { get; }
    }
}