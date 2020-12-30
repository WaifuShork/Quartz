namespace wsc.CodeAnalysis.Text
{
    /// <summary>
    /// Represents a TextSpan for the current input.
    /// </summary>
    public struct TextSpan
    {
        /// <summary>
        /// Constructor for TextSpan for a start and length.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }
        
        /// <summary>
        /// Represents the starting position of the input.
        /// </summary>
        public int Start { get; }
        
        /// <summary>
        /// Represents the length of the input.
        /// </summary>
        public int Length { get; }
        
        /// <summary>
        /// Represents the end/last character of the input.
        /// </summary>
        public int End => Start + Length;
        
        /// <summary>
        /// Represents the bounds of a currently selected input.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>Returns a TextSpan with Start and Length.</returns>
        public static TextSpan FromBounds(int start, int end)
        {
            var length = end - start;
            return new TextSpan(start, length);
        }

        /// <summary>
        /// Override of ToString to print Start and End positions.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Start}..{End}";
    }
}