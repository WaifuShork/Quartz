using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundCompoundFieldAssignmentExpression : BoundExpression
    {
        public BoundCompoundFieldAssignmentExpression(SyntaxNode syntax, BoundExpression classInstance, VariableSymbol classMember, BoundBinaryOperator op, BoundExpression expression)
            : base(syntax)
        {
            ClassInstance = classInstance;
            ClassMember = classMember;
            Op = op;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.CompoundFieldAssignmentExpression;
        public override TypeSymbol Type => Expression.Type;

        public BoundExpression ClassInstance { get; }
        public VariableSymbol ClassMember { get; }
        public BoundBinaryOperator Op {get; }
        public BoundExpression Expression { get; }
    }
}