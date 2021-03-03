namespace Vivian.CodeAnalysis.Symbols
{
    public class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new("?", null);
        public static readonly TypeSymbol Object = new("object", null);
        public static readonly TypeSymbol Bool = new("bool", false);
        public static readonly TypeSymbol Char = new("char", 0, isIntegral: true);
        public static readonly TypeSymbol Int8 = new("int8", 0, isIntegral: true);
        public static readonly TypeSymbol Int16 = new("int16", 0, isIntegral: true);
        public static readonly TypeSymbol Int32 = new("int32", 0, isIntegral: true);
        public static readonly TypeSymbol Int64 = new("int64", 0, isIntegral: true);
        public static readonly TypeSymbol UInt8 = new("uint8", 0, isIntegral: true);
        public static readonly TypeSymbol UInt16 = new("uint16", 0, isIntegral: true);
        public static readonly TypeSymbol UInt32 = new("uint32", 0, isIntegral: true);
        public static readonly TypeSymbol UInt64 = new("uint64", 0, isIntegral: true);
        public static readonly TypeSymbol Float32 = new("float32", 0, isFloat: true);
        public static readonly TypeSymbol Float64 = new("float64", 0, isFloat: true);
        public static readonly TypeSymbol Decimal = new("decimal", 0, isDecimal: true);
        public static readonly TypeSymbol String = new("string", string.Empty);
        public static readonly TypeSymbol Void = new("void", null);

        internal TypeSymbol(string name, object? defaultValue, bool isIntegral = false, bool isFloat = false, bool isDecimal = false)
            : base(name)
        {
            DefaultValue = defaultValue;
            IsIntegral = isIntegral;
            IsFloat = isFloat;
            IsDecimal = isDecimal;
        }

        public override SymbolKind Kind => SymbolKind.Type;

        public object? DefaultValue { get; }
        public bool IsIntegral { get; }
        public bool IsFloat { get; }
        public bool IsDecimal { get; }
        public bool IsNumeric => IsIntegral || IsFloat || IsDecimal;
    }
}