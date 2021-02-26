using System.Collections.Generic;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryOperator
        {
            private static List<BoundUnaryOperator> _operators = new List<BoundUnaryOperator>()
            {
                new BoundUnaryOperator(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, TypeSymbol.Bool),
            };
    
            static BoundUnaryOperator()
            {
                TypeSymbol[] numericTypes = {
                    TypeSymbol.Char,
                    TypeSymbol.Int8, TypeSymbol.Int16, TypeSymbol.Int32, TypeSymbol.Int64,
                    TypeSymbol.UInt8, TypeSymbol.UInt16, TypeSymbol.UInt32, TypeSymbol.UInt64,
                    TypeSymbol.Float32, TypeSymbol.Float64, TypeSymbol.Decimal
                };
    
                foreach (var type in numericTypes)
                {
                    _operators.Add(new BoundUnaryOperator(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, type));
                    _operators.Add(new BoundUnaryOperator(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, type));
                    _operators.Add(new BoundUnaryOperator(SyntaxKind.TildeToken, BoundUnaryOperatorKind.OnesComplement, type));
                }
            }
    
            private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol operandType)
                : this(syntaxKind, kind, operandType, operandType)
            {
            }
    
            private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType)
            {
                SyntaxKind = syntaxKind;
                Kind = kind;
                OperandType = operandType;
                Type = resultType;
            }
    
            public SyntaxKind SyntaxKind { get; }
            public BoundUnaryOperatorKind Kind { get; }
            public TypeSymbol OperandType { get; }
            public TypeSymbol Type { get; }
    
            public static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol operandType)
            {
                foreach (var op in _operators)
                {
                    if (op.SyntaxKind == syntaxKind && op.OperandType == operandType)
                        return op;
                }
    
                return null;
            }
        }
}