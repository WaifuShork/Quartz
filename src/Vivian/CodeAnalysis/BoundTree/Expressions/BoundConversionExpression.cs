using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(SyntaxNode syntax, TypeSymbol type, BoundExpression expression) : base(syntax)
        {
            Type = type;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
        public override TypeSymbol Type { get; }
        public BoundExpression Expression { get; }
    }
}