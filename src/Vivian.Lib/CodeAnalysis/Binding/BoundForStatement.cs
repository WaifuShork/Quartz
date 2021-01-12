using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundForStatement : BoundLoopStatement
    {
        public BoundForStatement(VariableSymbol variable, 
                                BoundExpression lowerBound, 
                                BoundExpression upperBound, 
                                BoundStatement body, 
                                BoundLabel breakLabel, 
                                BoundLabel continueLabel) : base(breakLabel, continueLabel)
        {
            Variable = variable;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Body = body;
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
        
        public VariableSymbol Variable { get; }
        public BoundExpression LowerBound { get; }
        public BoundExpression UpperBound { get; }
        public BoundStatement Body { get; }
        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}