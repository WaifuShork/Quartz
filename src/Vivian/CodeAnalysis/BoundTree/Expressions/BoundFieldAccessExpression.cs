using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundFieldAccessExpression : BoundExpression
    {
        public BoundFieldAccessExpression(SyntaxNode syntax, BoundExpression classInstance, VariableSymbol classMember)
            : base(syntax)
        {
            ClassInstance = classInstance;
            ClassMember = classMember;
        }

        public override BoundNodeKind Kind => BoundNodeKind.FieldAccessExpression;

        public BoundExpression ClassInstance { get; }
        public VariableSymbol ClassMember { get; }
        public override TypeSymbol Type => ClassMember.Type;
    }
}