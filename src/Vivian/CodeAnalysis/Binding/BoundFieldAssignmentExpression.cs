using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundFieldAssignmentExpression : BoundExpression
    {
        public BoundFieldAssignmentExpression(SyntaxNode syntax, BoundExpression structInstance, VariableSymbol structMember, BoundExpression expression)
            : base(syntax)
        {
            StructInstance = structInstance;
            StructMember = structMember;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.FieldAssignmentExpression;
        public override TypeSymbol Type => Expression.Type;
        public BoundExpression StructInstance { get; }
        public VariableSymbol StructMember { get; }
        public BoundExpression Expression { get; }
    }
}