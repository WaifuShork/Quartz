using Vivian.CodeAnalysis.Text;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class SyntaxTrivia
    {
        public SyntaxTrivia(SyntaxTree syntaxTree, SyntaxKind kind, int position, string text)
        {
            SyntaxTree = syntaxTree;
            Kind = kind;
            Position = position;
            Text = text;
        }

        public TextSpan Span => new TextSpan(Position, Text?.Length ?? 0);

        public SyntaxTree SyntaxTree { get; }
        public SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
    }
}