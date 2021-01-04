using wsc.CodeAnalysis.Symbols;

namespace wsc.CodeAnalysis.Binding
{
    internal sealed class BoundErrorExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
        public override TypeSymbol Type => TypeSymbol.Error;
    }
}