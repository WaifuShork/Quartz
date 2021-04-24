using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundThisExpression : BoundExpression
    {
        public BoundThisExpression(SyntaxNode syntax, ClassSymbol instance)
            : base(syntax)
        {
            Instance = instance;
        }

        public override TypeSymbol Type => Instance;
        public override BoundNodeKind Kind => BoundNodeKind.ThisExpression;
        
        public ClassSymbol Instance { get; }
    }
}