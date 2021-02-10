using Vivian.CodeAnalysis.Text;
using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class SyntaxToken : SyntaxNode
    {
        public SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, int position, string text, object value, TypeSymbol type) : base(syntaxTree)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
            Type = type;
        }

        public override SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }
        public TypeSymbol Type { get; }
        
        public override TextSpan Span => new(Position, Text?.Length ?? 0);

        public bool IsMissing => Text == null;
    }
}