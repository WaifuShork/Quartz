using System;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeComparator comp, Operation operation) 
            : this(syntaxKind, kind, comp, comp, null, operation)
        {
            
        }
        
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeComparator comp, TypeSymbol resultType, Operation operation) 
            : this(syntaxKind, kind, comp, comp, resultType, operation)
        {
            
        }
        
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeComparator leftComp, TypeComparator rightComp, TypeSymbol type, Operation operation)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            LeftComp = leftComp;
            RightComp = rightComp;
            Type = type;
            Operate = operation;
        }
        public SyntaxKind SyntaxKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public TypeComparator LeftComp { get; }
        public TypeComparator RightComp { get; }
        public TypeSymbol Type { get; }
        public Operation Operate { get; }

        private static BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, new TypeComparator(TypeSymbolCaps.Arithmetic), (l, r) => l + r),
            new BoundBinaryOperator(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, new TypeComparator(TypeSymbolCaps.Arithmetic), (l, r) => l - r),
            new BoundBinaryOperator(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, new TypeComparator(TypeSymbolCaps.Arithmetic), (l, r) => l * r),
            new BoundBinaryOperator(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, new TypeComparator(TypeSymbolCaps.Arithmetic), (l, r) => l / r),
            new BoundBinaryOperator(SyntaxKind.ModuloToken, BoundBinaryOperatorKind.Modulo, new TypeComparator(TypeSymbolCaps.Arithmetic), (l, r) => l % r),

            new BoundBinaryOperator(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, new TypeComparator(TypeSymbolCaps.FixedPoint), (l, r) => l & r),
            new BoundBinaryOperator(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, new TypeComparator(TypeSymbolCaps.FixedPoint), (l, r) => l | r),
            new BoundBinaryOperator(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, new TypeComparator(TypeSymbolCaps.FixedPoint), (l, r) => l ^ r),
            new BoundBinaryOperator(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, new TypeComparator(TypeSymbolCaps.FixedPoint), TypeSymbol.Bool, (l, r) => l == 0 ? (byte)0 : r == 0 ? (byte)0 : (byte)1),
            new BoundBinaryOperator(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, new TypeComparator(TypeSymbolCaps.FixedPoint), TypeSymbol.Bool, (l, r) => l == 0 ? r == 0 ? (byte)0 : (byte)1 : (byte)1),

            new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, new TypeComparator(TypeSymbolCaps.All), TypeSymbol.Bool, (l, r) => l == r ? (byte)1 : (byte)0),
            new BoundBinaryOperator(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, new TypeComparator(TypeSymbolCaps.All), TypeSymbol.Bool, (l, r) => l != r ? (byte)1 : (byte)0),

            new BoundBinaryOperator(SyntaxKind.LessToken, BoundBinaryOperatorKind.Less, new TypeComparator(TypeSymbolCaps.Comparable), TypeSymbol.Bool, (l, r) => l < r ? (byte)1 : (byte)0),
            new BoundBinaryOperator(SyntaxKind.LessOrEqualsToken, BoundBinaryOperatorKind.LessOrEquals, new TypeComparator(TypeSymbolCaps.Comparable), TypeSymbol.Bool, (l, r) => l <= r ? (byte)1 : (byte)0),
            new BoundBinaryOperator(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.Greater, new TypeComparator(TypeSymbolCaps.Comparable), TypeSymbol.Bool, (l, r) => l > r ? (byte)1 : (byte)0),
            new BoundBinaryOperator(SyntaxKind.GreaterOrEqualsToken, BoundBinaryOperatorKind.GreaterOrEquals, new TypeComparator(TypeSymbolCaps.Comparable), TypeSymbol.Bool, (l, r) => l >= r ? (byte)1 : (byte)0),

            new BoundBinaryOperator(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, new TypeComparator(TypeSymbolCaps.Array), (l, r) => l + r),
        };

        public delegate object Operation(dynamic left, dynamic right);

        public static BoundBinaryOperator Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
        {
            foreach (var op in _operators)
            {
                if (op.SyntaxKind == syntaxKind && op.LeftComp.Test(leftType) && op.RightComp.Test(rightType))
                {
                    return op;
                }
            }

            return null;
        }
    }
}