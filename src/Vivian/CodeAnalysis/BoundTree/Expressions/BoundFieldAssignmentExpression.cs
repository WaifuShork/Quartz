using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundFieldAssignmentExpression : BoundExpression
    {
        public BoundFieldAssignmentExpression(SyntaxNode syntax, BoundExpression classInstance, VariableSymbol classMember, BoundExpression expression)
            : base(syntax)
        {
            ClassInstance = classInstance;
            ClassMember = classMember;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.FieldAssignmentExpression;
        public override TypeSymbol Type => Expression.Type;
        
        public BoundExpression ClassInstance { get; }
        public VariableSymbol ClassMember { get; }
        public BoundExpression Expression { get; }
    }
}