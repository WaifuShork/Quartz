using System;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(SyntaxNode syntax, object? value)
            : base(syntax)
        {
            switch (value)
            {
                case bool:
                    Type = TypeSymbol.Bool;
                    break;
                case sbyte:
                    Type = TypeSymbol.Int8;
                    break;
                case short:
                    Type = TypeSymbol.Int16;
                    break;
                case int:
                    Type = TypeSymbol.Int32;
                    break;
                case long:
                    Type = TypeSymbol.Int64;
                    break;
                case byte:
                    Type = TypeSymbol.Int8;
                    break;
                case ushort:
                    Type = TypeSymbol.UInt16;
                    break;
                case uint:
                    Type = TypeSymbol.UInt32;
                    break;
                case ulong:
                    Type = TypeSymbol.UInt64;
                    break;
                case float:
                    Type = TypeSymbol.Float32;
                    break;
                case double:
                    Type = TypeSymbol.Float64;
                    break;
                case decimal:
                    Type = TypeSymbol.Decimal;
                    break;
                case char:
                    Type = TypeSymbol.Char;
                    break;
                case string:
                    Type = TypeSymbol.String;
                    break;
                case null:
                    Type = TypeSymbol.Void;
                    break;
                default:
                    throw new Exception($"Unexpected literal '{value}' of type {value.GetType()}");
            }

            ConstantValue = new BoundConstant(value);
        }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override TypeSymbol Type { get; }
        public object? Value => ConstantValue.Value;
        public override BoundConstant ConstantValue { get; }
    }
}