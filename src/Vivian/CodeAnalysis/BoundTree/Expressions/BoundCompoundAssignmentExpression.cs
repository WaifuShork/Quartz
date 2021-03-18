using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundCompoundAssignmentExpression : BoundExpression
    {
        public BoundCompoundAssignmentExpression(SyntaxNode syntax, VariableSymbol variable, BoundBinaryOperator op, BoundExpression expression)
            : base(syntax)
        {
            Variable = variable;
            Op = op;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.CompoundAssignmentExpression;
        public override TypeSymbol Type => Expression.Type;
        public VariableSymbol Variable { get; }
        public BoundBinaryOperator Op {get; }
        public BoundExpression Expression { get; }
    }
}