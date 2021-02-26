using System;

namespace Vivian.CodeAnalysis
{
    public class Position : IComparable
    {
        public Position(int line, int character)
        {
            Line = line;
            Character = character;
        }

        public int Line { get; set; }
        public int Character { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null) { return 1; }
            if (obj is Position p) {
                var result = Line.CompareTo(p.Line);

                if (result == 0) {
                    return Character.CompareTo(p.Character);
                }

                return result;
            } else {
                throw new InvalidOperationException($"Can't compare position with {obj.GetType()}");
            }
        }
    }
}