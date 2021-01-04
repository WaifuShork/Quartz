using System;
using wsc.CodeAnalysis.Symbols;

namespace wsc.CodeAnalysis.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract TypeSymbol Type { get; }
    }
}