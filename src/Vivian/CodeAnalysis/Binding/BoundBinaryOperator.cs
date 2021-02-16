using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol type) 
            : this(syntaxKind, kind, type, type, type)
        {
            
        }
        
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType) 
            : this(syntaxKind, kind, operandType, operandType, resultType)
        {
            
        }
        
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol type)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            Type = type;
        }
        
        public SyntaxKind SyntaxKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol Type { get; }

        private static readonly BoundBinaryOperator[] _operators =
        {
            new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Int),
            new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Int),
            new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Int),
            new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Int),
            new(SyntaxKind.ModuloToken, BoundBinaryOperatorKind.Modulo, TypeSymbol.Int),
            
            new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Int),
            new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Int),
            new(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Int),
            
            new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.String, TypeSymbol.Bool),

            new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.String, TypeSymbol.Bool),

            new(SyntaxKind.LessToken, BoundBinaryOperatorKind.Less, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.LessOrEqualsToken, BoundBinaryOperatorKind.LessOrEquals, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.Greater, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.GreaterOrEqualsToken, BoundBinaryOperatorKind.GreaterOrEquals, TypeSymbol.Int, TypeSymbol.Bool),

            new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
            new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
            new(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Bool),
            new(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, TypeSymbol.Bool),
            new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.Bool),
            new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Bool),
            new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Bool),
            
            new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.String),
            
            new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Object),
            new (SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Object)
        };

        public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
        {
            foreach (var op in _operators)
            {
                if (op.SyntaxKind == syntaxKind && op.LeftType == leftType && op.RightType == rightType)
                    return op;
            }

            return null;
        }
    }
}