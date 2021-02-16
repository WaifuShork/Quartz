namespace Vivian.CodeAnalysis.Text
{
    public readonly struct TextLocation
    {
        public TextLocation(SourceText text, TextSpan span)
        {
            Text = text;
            Span = span;
        }
        
        public SourceText Text { get; }
        public TextSpan Span { get; }

        public string FileName => Text.FileName;
        public int StartLine => Text.GetLineIndex(Span.Start);
        public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;

        public int EndLine => Text.GetLineIndex(Span.End);
        public int EndCharacter => Span.End - Text.Lines[StartLine].Start;
    }
}