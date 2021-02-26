namespace Vivian.CodeAnalysis.Symbols
{
    public class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new TypeSymbol("?", null);
        public static readonly TypeSymbol Object = new TypeSymbol("object", null);
        public static readonly TypeSymbol Bool = new TypeSymbol("bool", false);
        public static readonly TypeSymbol Char = new TypeSymbol("char", 0, isIntegral: true);
        public static readonly TypeSymbol Int8 = new TypeSymbol("int8", 0, isIntegral: true);
        public static readonly TypeSymbol Int16 = new TypeSymbol("int16", 0, isIntegral: true);
        public static readonly TypeSymbol Int32 = new TypeSymbol("int32", 0, isIntegral: true);
        public static readonly TypeSymbol Int64 = new TypeSymbol("int64", 0, isIntegral: true);
        public static readonly TypeSymbol UInt8 = new TypeSymbol("uint8", 0, isIntegral: true);
        public static readonly TypeSymbol UInt16 = new TypeSymbol("uint16", 0, isIntegral: true);
        public static readonly TypeSymbol UInt32 = new TypeSymbol("uint32", 0, isIntegral: true);
        public static readonly TypeSymbol UInt64 = new TypeSymbol("uint64", 0, isIntegral: true);
        public static readonly TypeSymbol Float32 = new TypeSymbol("float32", 0, isFloat: true);
        public static readonly TypeSymbol Float64 = new TypeSymbol("float64", 0, isFloat: true);
        public static readonly TypeSymbol Decimal = new TypeSymbol("decimal", 0, isDecimal: true);
        public static readonly TypeSymbol String = new TypeSymbol("string", string.Empty);
        public static readonly TypeSymbol Void = new TypeSymbol("void", null);

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