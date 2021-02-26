using System;

using Vivian.CodeAnalysis.Binding;

namespace Vivian.CodeAnalysis.Binding
{
    internal static class ConstantFolding
    {
        public static BoundConstant? Fold(BoundUnaryOperator op, BoundExpression operand)
        {
            if (operand.ConstantValue?.Value != null)
            {
                return op.Kind switch
                {
                    BoundUnaryOperatorKind.Identity => new BoundConstant(operand.ConstantValue.Value switch
                        {
                            char i => i,
                            sbyte i => i,
                            short i => i,
                            int i => i,
                            long i => i,
                            byte i => i,
                            ushort i => i,
                            uint i => i,
                            ulong i => i,
                            float i => i,
                            double i => i,
                            decimal i => i,
                            _ => throw new Exception("Unexpected type")
                        }),
                    BoundUnaryOperatorKind.Negation => new BoundConstant(operand.ConstantValue.Value switch
                        {
                            char i => -i,
                            sbyte i => -i,
                            short i => -i,
                            int i => -i,
                            long i => -i,
                            byte i => -i,
                            float i => -i,
                            double i => -i,
                            decimal i => -i,
                            _ => throw new Exception("Unexpected type")
                        }),
                    BoundUnaryOperatorKind.LogicalNegation => new BoundConstant(!(bool)operand.ConstantValue.Value),
                    BoundUnaryOperatorKind.OnesComplement => new BoundConstant(operand.ConstantValue.Value switch
                        {
                            char i => ~i,
                            sbyte i => ~i,
                            short i => ~i,
                            int i => ~i,
                            long i => ~i,
                            byte i => ~i,
                            ushort i => ~i,
                            uint i => ~i,
                            ulong i => ~i,
                            _ => throw new Exception("Unexpected type")
                        }),
                    _ => throw new Exception($"Unexpected unary operator {op.Kind}")
                };
            }

            return null;
        }

        public static BoundConstant? Fold(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
        {
            var leftConstant = left.ConstantValue;
            var rightConstant = right.ConstantValue;

            // Special case && and || because there are cases where only one
            // side needs to be known.

            if (op.Kind == BoundBinaryOperatorKind.LogicalAnd)
            {
                if ((leftConstant?.Value != null && !(bool)leftConstant.Value) ||
                    (rightConstant?.Value != null && !(bool)rightConstant.Value))
                {
                    return new BoundConstant(false);
                }
            }

            if (op.Kind == BoundBinaryOperatorKind.LogicalOr)
            {
                if ((leftConstant?.Value != null && (bool)leftConstant.Value) ||
                    (rightConstant?.Value != null && (bool)rightConstant.Value))
                {
                    return new BoundConstant(true);
                }
            }

            if (leftConstant?.Value == null || rightConstant?.Value == null)
                return null;


            var l = leftConstant.Value;
            var r = rightConstant.Value;

            switch (op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (left.Type.IsNumeric)
                        return new BoundConstant(l switch
                        {
                            char i => i + (char)r,
                            sbyte i => i + (sbyte)r,
                            short i => i + (short)r,
                            int i => i + (int)r,
                            long i => i + (long)r,
                            byte i => i + (byte)r,
                            ushort i => i + (ushort)r,
                            uint i => i + (uint)r,
                            ulong i => i + (ulong)r,
                            float i => i + (float)r,
                            double i => i + (double)r,
                            decimal i => i + (decimal)r,
                            _ => throw new Exception("Unexpected type")
                        });
                    else
                        return new BoundConstant((string)l + (string)r);
                case BoundBinaryOperatorKind.Subtraction:
                    return new BoundConstant((int)l - (int)r);
                case BoundBinaryOperatorKind.Multiplication:
                    return new BoundConstant((int)l * (int)r);
                case BoundBinaryOperatorKind.Division:
                    return new BoundConstant((int)l / (int)r);
                case BoundBinaryOperatorKind.BitwiseAnd:
                    if (left.Type.IsNumeric)
                        return new BoundConstant(l switch
                        {
                            char i => i & (char)r,
                            sbyte i => i & (sbyte)r,
                            short i => i & (short)r,
                            int i => i & (int)r,
                            long i => i & (long)r,
                            byte i => i & (byte)r,
                            ushort i => i & (ushort)r,
                            uint i => i & (uint)r,
                            ulong i => i & (ulong)r,

                            // Can't do bitwise operations on floats

                            _ => throw new Exception("Unexpected type")
                        });
                    else
                        return new BoundConstant((bool)l && (bool)r);
                case BoundBinaryOperatorKind.BitwiseOr:
                    if (left.Type.IsNumeric)
                        return new BoundConstant(l switch
                        {
                            char i => i | (char)r,
                            sbyte i => i | (sbyte)r,
                            short i => i | (short)r,
                            int i => i | (int)r,
                            long i => i | (long)r,
                            byte i => i | (byte)r,
                            ushort i => i | (ushort)r,
                            uint i => i | (uint)r,
                            ulong i => i | (ulong)r,

                            // Can't do bitwise operations on floats

                            _ => throw new Exception("Unexpected type")
                        });
                    else
                        return new BoundConstant((bool)l || (bool)r);
                case BoundBinaryOperatorKind.BitwiseXor:
                    if (left.Type.IsNumeric)
                        return new BoundConstant(l switch
                        {
                            char i => i ^ (char)r,
                            sbyte i => i ^ (sbyte)r,
                            short i => i ^ (short)r,
                            int i => i ^ (int)r,
                            long i => i ^ (long)r,
                            byte i => i ^ (byte)r,
                            ushort i => i ^ (ushort)r,
                            uint i => i ^ (uint)r,
                            ulong i => i ^ (ulong)r,

                            // Can't do bitwise operations on floats

                            _ => throw new Exception("Unexpected type")
                        });
                    else
                        return new BoundConstant((bool)l ^ (bool)r);
                case BoundBinaryOperatorKind.LogicalAnd:
                    return new BoundConstant((bool)l && (bool)r);
                case BoundBinaryOperatorKind.LogicalOr:
                    return new BoundConstant((bool)l || (bool)r);
                case BoundBinaryOperatorKind.Equals:
                    return new BoundConstant(Equals(l, r));
                case BoundBinaryOperatorKind.NotEquals:
                    return new BoundConstant(!Equals(l, r));
                case BoundBinaryOperatorKind.Less:
                        return new BoundConstant(l switch
                        {
                            char i => i < (char)r,
                            sbyte i => i < (sbyte)r,
                            short i => i < (short)r,
                            int i => i < (int)r,
                            long i => i < (long)r,
                            byte i => i < (byte)r,
                            ushort i => i < (ushort)r,
                            uint i => i < (uint)r,
                            ulong i => i < (ulong)r,
                            float i => i < (float)r,
                            double i => i < (double)r,
                            decimal i => i < (decimal)r,
                            _ => throw new Exception("Unexpected type")
                        });
                case BoundBinaryOperatorKind.LessOrEquals:
                        return new BoundConstant(l switch
                        {
                            char i => i <= (char)r,
                            sbyte i => i <= (sbyte)r,
                            short i => i <= (short)r,
                            int i => i <= (int)r,
                            long i => i <= (long)r,
                            byte i => i <= (byte)r,
                            ushort i => i <= (ushort)r,
                            uint i => i <= (uint)r,
                            ulong i => i <= (ulong)r,
                            float i => i <= (float)r,
                            double i => i <= (double)r,
                            decimal i => i <= (decimal)r,
                            _ => throw new Exception("Unexpected type")
                        });
                case BoundBinaryOperatorKind.Greater:
                        return new BoundConstant(l switch
                        {
                            char i => i > (char)r,
                            sbyte i => i > (sbyte)r,
                            short i => i > (short)r,
                            int i => i > (int)r,
                            long i => i > (long)r,
                            byte i => i > (byte)r,
                            ushort i => i > (ushort)r,
                            uint i => i > (uint)r,
                            ulong i => i > (ulong)r,
                            float i => i > (float)r,
                            double i => i > (double)r,
                            decimal i => i > (decimal)r,
                            _ => throw new Exception("Unexpected type")
                        });
                case BoundBinaryOperatorKind.GreaterOrEquals:
                        return new BoundConstant(l switch
                        {
                            char i => i >= (char)r,
                            sbyte i => i >= (sbyte)r,
                            short i => i >= (short)r,
                            int i => i >= (int)r,
                            long i => i >= (long)r,
                            byte i => i >= (byte)r,
                            ushort i => i >= (ushort)r,
                            uint i => i >= (uint)r,
                            ulong i => i >= (ulong)r,
                            float i => i >= (float)r,
                            double i => i >= (double)r,
                            decimal i => i >= (decimal)r,
                            _ => throw new Exception("Unexpected type")
                        });
                default:
                    throw new Exception($"Unexpected binary operator {op.Kind}");
            }
        }
    }
}
