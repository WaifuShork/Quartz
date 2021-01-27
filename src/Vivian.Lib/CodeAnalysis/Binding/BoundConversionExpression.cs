using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Binding
{
    internal class BoundConversionExpression : BoundExpression
    {

        public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
        {
            Type = type;
            Expression = expression;
        }
        
        public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
        public override TypeSymbol Type { get; }
        public BoundExpression Expression { get; }

    }
}