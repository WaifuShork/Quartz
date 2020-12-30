using System.Collections.Immutable;

namespace wsc.CodeAnalysis.Text
{
    /// <summary>
    /// Represents the current SourceText being parsed
    /// </summary>
    public sealed class SourceText
    {
        private readonly string _text;
        private SourceText(string text)
        {
            Lines = ParseLines(this, text);
            _text = text;
        }
        
        /// <summary>
        /// The lines being parsed
        /// </summary>
        public ImmutableArray<TextLine> Lines { get; }

        public char this[int index] => _text[index];

        /// <summary>
        /// Length of the current section of text being parsed
        /// </summary>
        public int Length => _text.Length;

        /// <summary>
        /// Gets the current index of character that throws InvalidToken
        /// </summary>
        /// <param name="position"></param>
        /// <returns>index</returns>
        public int GetLineIndex(int position)
        {
            var lower = 0;
            var upper = Lines.Length - 1;

            while (lower <= upper)
            {
                var index = lower + (upper - 1) / 2;
                var start = Lines[index].Start;

                if (position == start)
                    return index;
                
                if (start > position)
                {
                    upper = index - 1;
                }
                else
                {
                    lower = index + 1;
                }
            }

            return lower - 1;
        }

        /// <summary>
        /// Parses a given source text to build errors 
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="text"></param>
        /// <returns>text line</returns>
        private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
        {
            var result = ImmutableArray.CreateBuilder<TextLine>();

            var position = 0;
            var lineStart = 0;
            
            while (position < text.Length)
            {
                var lineBreakWidth = GetLineBreakWidth(text, position);

                if (lineBreakWidth == 0)
                {
                    position++;
                }
                else
                {
                    AddLine(result,sourceText, position, lineStart, lineBreakWidth);

                    position += lineBreakWidth;
                    lineStart = position;
                }
            }

            if (position >= lineStart)
                AddLine(result, sourceText, position, lineStart, 0);
            
            return result.ToImmutable();
        }
        
        private static void AddLine(ImmutableArray<TextLine>.Builder result, SourceText sourceText, int position, int lineStart, int lineBreakWidth)
        {
            var lineLength = position - lineStart;
            var lineLengthIncludingLineBreak = lineLength + lineBreakWidth;
            var line = new TextLine(sourceText, lineStart, lineLength, lineLengthIncludingLineBreak);
            result.Add(line);
        }
        
        /// <summary>
        /// Gets the break of a line width for a given text and position input
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <returns>0, 1, or 2</returns>
        private static int GetLineBreakWidth(string text, int position)
        {
            var c = text[position];
            var l = position + 1 >= text.Length ? '\0' : text[position + 1];

            if (c == '\r' && l == '\n')
                return 2;

            if (c == '\r' || c == '\n')
                return 1;

            return 0;
        }

        /// <summary>
        /// Returns a new SourceText to be used
        /// </summary>
        /// <param name="text"></param>
        /// <returns>SourceText</returns>
        public static SourceText From(string text)
        {
            return new SourceText(text);
        }

        public override string ToString() => _text;

        public string ToString(int start, int length) => _text.Substring(start, length);

        public string ToString(TextSpan span) => ToString(span.Start, span.Length);
    }
}