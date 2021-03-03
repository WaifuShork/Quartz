namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundConstant
    {
        public BoundConstant(object? value)
        {
            Value = value;
        }

        public object? Value { get; }
        public bool IsZero => Value switch 
        {
            char i => i == 0,
            sbyte i => i == 0,
            short i => i == 0,
            int i => i == 0,
            long i => i == 0,
            byte i => i == 0,
            ushort i => i == 0,
            uint i => i == 0,
            ulong i => i == 0,
            float i => i == 0,
            double i => i == 0,
            decimal i => i == 0,
            _ => false
        };
    }
}