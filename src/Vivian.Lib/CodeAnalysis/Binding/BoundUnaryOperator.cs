using System;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryOperator
    {
        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeComparator operandComp, Operation operation) : this(syntaxKind, kind, operandComp, null, operation)
        {
        }
        
        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeComparator operandComp, TypeSymbol returnType, Operation operation)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            OperandComp = operandComp;
            Type = returnType;
            Operate = operation;
        }
        public SyntaxKind SyntaxKind { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public TypeComparator OperandComp { get; }
        public TypeSymbol Type { get; }
        public Operation Operate { get; }

        private static BoundUnaryOperator[] _operators =
        {
            new BoundUnaryOperator(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, new TypeComparator(TypeSymbolCaps.Arithmetic), TypeSymbol.Bool, x => x == 0 ? (byte)1 : (byte)0),

            new BoundUnaryOperator(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, new TypeComparator(TypeSymbolCaps.Arithmetic), x => x),
            new BoundUnaryOperator(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, new TypeComparator(TypeSymbolCaps.Arithmetic), x => -x),

            new BoundUnaryOperator(SyntaxKind.TildeToken, BoundUnaryOperatorKind.OnesComplement, new TypeComparator(TypeSymbol.Bool), x => (byte)~x),
            new BoundUnaryOperator(SyntaxKind.TildeToken, BoundUnaryOperatorKind.OnesComplement, new TypeComparator(TypeSymbolCaps.FixedPoint), x => ~x),
        };

        public delegate object Operation(dynamic _in);

        public static BoundUnaryOperator Bind(SyntaxKind syntaxKind, TypeSymbol operandType)
        {
            foreach (var op in _operators)
            {
                if (op.SyntaxKind == syntaxKind && op.OperandComp.Test(operandType))
                    return op;
            }
            return null;
        }
    }
}