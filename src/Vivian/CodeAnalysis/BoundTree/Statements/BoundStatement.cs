using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal abstract class BoundStatement : BoundNode
    {
        protected BoundStatement(SyntaxNode syntax) : base(syntax) { }
    }
}