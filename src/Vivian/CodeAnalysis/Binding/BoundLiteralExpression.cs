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
            if (value is bool)
                Type = TypeSymbol.Bool;
            else if (value is sbyte)
                Type = TypeSymbol.Int8;
            else if (value is short)
                Type = TypeSymbol.Int16;
            else if (value is int)
                Type = TypeSymbol.Int32;
            else if (value is long)
                Type = TypeSymbol.Int64;
            else if (value is byte)
                Type = TypeSymbol.Int8;
            else if (value is ushort)
                Type = TypeSymbol.UInt16;
            else if (value is uint)
                Type = TypeSymbol.UInt32;
            else if (value is ulong)
                Type = TypeSymbol.UInt64;
            else if (value is float)
                Type = TypeSymbol.Float32;
            else if (value is double)
                Type = TypeSymbol.Float64;
            else if (value is decimal)
                Type = TypeSymbol.Decimal;
            else if (value is char)
                Type = TypeSymbol.Char;
            else if (value is string)
                Type = TypeSymbol.String;
            else if (value is null)
                Type = TypeSymbol.Void;
            else
                throw new Exception($"Unexpected literal '{value}' of type {value.GetType()}");

            ConstantValue = new BoundConstant(value);
        }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override TypeSymbol Type { get; }
        public object? Value => ConstantValue.Value;
        public override BoundConstant ConstantValue { get; }
    }
}