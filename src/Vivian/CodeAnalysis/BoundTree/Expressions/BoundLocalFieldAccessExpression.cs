using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundLocalFieldAccessExpression : BoundExpression
    {
        public BoundLocalFieldAccessExpression(SyntaxNode syntax, ClassSymbol instance, VariableSymbol classMember)
            : base(syntax)
        {
            Instance = instance;
            ClassMember = classMember;
        }

        public override TypeSymbol Type => ClassMember.Type;
        public override BoundNodeKind Kind => BoundNodeKind.ThisExpression;
        
        public ClassSymbol Instance { get; }
        public VariableSymbol ClassMember { get; }
    }
}