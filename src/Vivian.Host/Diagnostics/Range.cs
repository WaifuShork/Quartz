namespace Vivian.CodeAnalysis
{
    public class Range
    {
        public Range(Position start, Position end)
        {
            Start = start;
            End = end;
        }

        public Position Start { get; set; }
        public Position End { get; set; }
    }
}