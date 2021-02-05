using System;
using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value)
        {
            Value = value;
            switch (value)
            {
                case int:
                    Type = TypeSymbol.Int;
                    break;
                case string:
                    Type = TypeSymbol.String;
                    break;
                default:
                    throw new Exception($"Unexpected literal <{value}> of type {value.GetType()}");
            }
        }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override TypeSymbol Type { get; }
        
        public object Value { get; }
    }
}