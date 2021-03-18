using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundNopStatement : BoundStatement
    {
        public BoundNopStatement(SyntaxNode syntax)
            : base(syntax) { }

        public override BoundNodeKind Kind => BoundNodeKind.NopStatement;
    }
}