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
                        _ => throw new InternalCompilerException("Unexpected type")
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
                        _ => throw new InternalCompilerException("Unexpected type")
                    }),
                    BoundUnaryOperatorKind.LogicalNegation => new BoundConstant(!(bool) operand.ConstantValue.Value),
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
                        _ => throw new InternalCompilerException("Unexpected type")
                    }),
                    _ => throw new InternalCompilerException($"Unexpected unary operator {op.Kind}")
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
                if (leftConstant?.Value != null && !(bool)leftConstant.Value ||
                    rightConstant?.Value != null && !(bool)rightConstant.Value)
                {
                    return new BoundConstant(false);
                }
            }

            if (op.Kind == BoundBinaryOperatorKind.LogicalOr)
            {
                if (leftConstant?.Value != null && (bool)leftConstant.Value ||
                    rightConstant?.Value != null && (bool)rightConstant.Value)
                {
                    return new BoundConstant(true);
                }
            }

            if (leftConstant?.Value == null || rightConstant?.Value == null)
            {
                return null;
            }


            var leftConstantValue = leftConstant.Value;
            var rightConstantValue = rightConstant.Value;

            switch (op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (left.Type.IsNumeric)
                    {
                        return new BoundConstant(leftConstantValue switch
                        {
                            char i => i + (char)rightConstantValue,
                            sbyte i => i + (sbyte)rightConstantValue,
                            short i => i + (short)rightConstantValue,
                            int i => i + (int)rightConstantValue,
                            long i => i + (long)rightConstantValue,
                            byte i => i + (byte)rightConstantValue,
                            ushort i => i + (ushort)rightConstantValue,
                            uint i => i + (uint)rightConstantValue,
                            ulong i => i + (ulong)rightConstantValue,
                            float i => i + (float)rightConstantValue,
                            double i => i + (double)rightConstantValue,
                            decimal i => i + (decimal)rightConstantValue,
                            _ => throw new InternalCompilerException("Unexpected type")
                        });
                    }
                    else
                    {
                        return new BoundConstant((string)leftConstantValue + (string)rightConstantValue);
                    }
                
                case BoundBinaryOperatorKind.Subtraction:
                    return new BoundConstant((int) leftConstantValue - (int) rightConstantValue);
                case BoundBinaryOperatorKind.Multiplication:
                    return new BoundConstant((int) leftConstantValue * (int) rightConstantValue);
                case BoundBinaryOperatorKind.Division:
                    return new BoundConstant((int) leftConstantValue / (int) rightConstantValue);
                case BoundBinaryOperatorKind.Modulo:
                    return new BoundConstant((int) leftConstantValue % (int) rightConstantValue);
                case BoundBinaryOperatorKind.BitwiseAnd:
                    if (left.Type.IsNumeric)
                    {
                        return new BoundConstant(leftConstantValue switch
                        {
                            char i => i & (char) rightConstantValue,
                            sbyte i => i & (sbyte) rightConstantValue,
                            short i => i & (short) rightConstantValue,
                            int i => i & (int) rightConstantValue,
                            long i => i & (long) rightConstantValue,
                            byte i => i & (byte) rightConstantValue,
                            ushort i => i & (ushort) rightConstantValue,
                            uint i => i & (uint) rightConstantValue,
                            ulong i => i & (ulong) rightConstantValue,

                            // Can't do bitwise operations on floats

                            _ => throw new InternalCompilerException("Unexpected type")
                        });
                    }
                    else
                    {
                        return new BoundConstant((bool)leftConstantValue && (bool)rightConstantValue);
                    }
                
                case BoundBinaryOperatorKind.BitwiseOr:
                    if (left.Type.IsNumeric)
                    {
                        return new BoundConstant(leftConstantValue switch
                        {
                            char i => i | (char)rightConstantValue,
                            sbyte i => i | (sbyte)rightConstantValue,
                            short i => i | (short)rightConstantValue,
                            int i => i | (int)rightConstantValue,
                            long i => i | (long)rightConstantValue,
                            byte i => i | (byte)rightConstantValue,
                            ushort i => i | (ushort)rightConstantValue,
                            uint i => i | (uint)rightConstantValue,
                            ulong i => i | (ulong)rightConstantValue,

                            // Can't do bitwise operations on floats

                            _ => throw new InternalCompilerException("Unexpected type")
                        });
                    }
                    else
                    {
                        return new BoundConstant((bool)leftConstantValue || (bool)rightConstantValue);
                    }
                
                case BoundBinaryOperatorKind.BitwiseXor:
                    if (left.Type.IsNumeric)
                    {
                        return new BoundConstant(leftConstantValue switch
                        {
                            char i => i ^ (char)rightConstantValue,
                            sbyte i => i ^ (sbyte)rightConstantValue,
                            short i => i ^ (short)rightConstantValue,
                            int i => i ^ (int)rightConstantValue,
                            long i => i ^ (long)rightConstantValue,
                            byte i => i ^ (byte)rightConstantValue,
                            ushort i => i ^ (ushort)rightConstantValue,
                            uint i => i ^ (uint)rightConstantValue,
                            ulong i => i ^ (ulong)rightConstantValue,

                            // Can't do bitwise operations on floats
                            _ => throw new InternalCompilerException("Unexpected type")
                        });
                    }
                    else
                    {
                        return new BoundConstant((bool)leftConstantValue ^ (bool)rightConstantValue);
                    }
                case BoundBinaryOperatorKind.LogicalAnd:
                    return new BoundConstant((bool)leftConstantValue && (bool)rightConstantValue);
                case BoundBinaryOperatorKind.LogicalOr:
                    return new BoundConstant((bool)leftConstantValue || (bool)rightConstantValue);
                case BoundBinaryOperatorKind.Equals:
                    return new BoundConstant(Equals(leftConstantValue, rightConstantValue));
                case BoundBinaryOperatorKind.NotEquals:
                    return new BoundConstant(!Equals(leftConstantValue, rightConstantValue));
                case BoundBinaryOperatorKind.Less:
                    return new BoundConstant(leftConstantValue switch
                    {
                        char i => i < (char)rightConstantValue,
                        sbyte i => i < (sbyte)rightConstantValue,
                        short i => i < (short)rightConstantValue,
                        int i => i < (int)rightConstantValue,
                        long i => i < (long)rightConstantValue,
                        byte i => i < (byte)rightConstantValue,
                        ushort i => i < (ushort)rightConstantValue,
                        uint i => i < (uint)rightConstantValue,
                        ulong i => i < (ulong)rightConstantValue,
                        float i => i < (float)rightConstantValue,
                        double i => i < (double)rightConstantValue,
                        decimal i => i < (decimal)rightConstantValue,
                        _ => throw new InternalCompilerException("Unexpected type")
                    });
                case BoundBinaryOperatorKind.LessOrEquals:
                    return new BoundConstant(leftConstantValue switch
                    {
                        char i => i <= (char)rightConstantValue,
                        sbyte i => i <= (sbyte)rightConstantValue,
                        short i => i <= (short)rightConstantValue,
                        int i => i <= (int)rightConstantValue,
                        long i => i <= (long)rightConstantValue,
                        byte i => i <= (byte)rightConstantValue,
                        ushort i => i <= (ushort)rightConstantValue,
                        uint i => i <= (uint)rightConstantValue,
                        ulong i => i <= (ulong)rightConstantValue,
                        float i => i <= (float)rightConstantValue,
                        double i => i <= (double)rightConstantValue,
                        decimal i => i <= (decimal)rightConstantValue,
                        _ => throw new InternalCompilerException("Unexpected type")
                    });
                case BoundBinaryOperatorKind.Greater:
                    return new BoundConstant(leftConstantValue switch
                    {
                        char i => i > (char)rightConstantValue,
                        sbyte i => i > (sbyte)rightConstantValue,
                        short i => i > (short)rightConstantValue,
                        int i => i > (int)rightConstantValue,
                        long i => i > (long)rightConstantValue,
                        byte i => i > (byte)rightConstantValue,
                        ushort i => i > (ushort)rightConstantValue,
                        uint i => i > (uint)rightConstantValue,
                        ulong i => i > (ulong)rightConstantValue,
                        float i => i > (float)rightConstantValue,
                        double i => i > (double)rightConstantValue,
                        decimal i => i > (decimal)rightConstantValue,
                        _ => throw new InternalCompilerException("Unexpected type")
                    });
                case BoundBinaryOperatorKind.GreaterOrEquals:
                    return new BoundConstant(leftConstantValue switch
                    {
                        char i => i >= (char)rightConstantValue,
                        sbyte i => i >= (sbyte)rightConstantValue,
                        short i => i >= (short)rightConstantValue,
                        int i => i >= (int)rightConstantValue,
                        long i => i >= (long)rightConstantValue,
                        byte i => i >= (byte)rightConstantValue,
                        ushort i => i >= (ushort)rightConstantValue,
                        uint i => i >= (uint)rightConstantValue,
                        ulong i => i >= (ulong)rightConstantValue,
                        float i => i >= (float)rightConstantValue,
                        double i => i >= (double)rightConstantValue,
                        decimal i => i >= (decimal)rightConstantValue,
                        _ => throw new InternalCompilerException("Unexpected type")
                    });
                default:
                    throw new InternalCompilerException($"Unexpected binary operator {op.Kind}");
            }
        }
    }
}
