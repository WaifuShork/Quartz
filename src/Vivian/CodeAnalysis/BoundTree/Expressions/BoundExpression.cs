using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        protected BoundExpression(SyntaxNode syntax) : base(syntax) { }

        public abstract TypeSymbol Type { get; }
        public virtual BoundConstant? ConstantValue => null;
    }
}