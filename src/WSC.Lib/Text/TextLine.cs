namespace wsc.CodeAnalysis.Text
{
    /// <summary>
    /// Represents a line of text.
    /// </summary>
    public sealed class TextLine
    {
        public TextLine(SourceText text, int start, int length, int lengthIncludingLineBreak)
        {
            Text = text;
            Start = start;
            Length = length;
            LengthIncludingLineBreak = lengthIncludingLineBreak;
        }
        
        /// <summary>
        /// Represents a SourceText.
        /// </summary>
        public SourceText Text { get; }
        
        /// <summary>
        /// Represents starting position of the line.
        /// </summary>
        public int Start { get; }
        
        /// <summary>
        /// Represents the length of the line.
        /// </summary>
        public int Length { get; }
        
        /// <summary>
        /// Represents the end of the line.
        /// </summary>
        public int End => Start + Length;
        
        /// <summary>
        /// Represents the length of the line including line breaks and carriage returns.
        /// </summary>
        public int LengthIncludingLineBreak { get; }
        
        /// <summary>
        /// Represents the current span of the given text input.
        /// </summary>
        /// <returns>Returns a new text span with the Start and Length.</returns>
        public TextSpan Span => new TextSpan(Start, Length);
        
        /// <summary>
        /// Represents the current span of the given text input including line breaks and carriage returns.
        /// </summary>
        public TextSpan SpanIncludingLineBreak => new TextSpan(Start, LengthIncludingLineBreak);
        
        /// <summary>
        /// Override ToString for TextSpan
        /// </summary>
        /// <returns>Text.ToString(Span)</returns>
        public override string ToString() => Text.ToString(Span);
    }
}