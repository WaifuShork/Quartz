using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundFieldAccessExpression : BoundExpression
    {
        public BoundFieldAccessExpression(SyntaxNode syntax, BoundExpression structInstance, VariableSymbol structMember)
            : base(syntax)
        {
            StructInstance = structInstance;
            StructMember = structMember;
        }

        public override BoundNodeKind Kind => BoundNodeKind.FieldAccessExpression;

        public BoundExpression StructInstance { get; }
        public VariableSymbol StructMember { get; }
        public override TypeSymbol Type => StructMember.Type;
    }
}