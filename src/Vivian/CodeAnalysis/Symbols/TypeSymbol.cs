namespace Vivian.CodeAnalysis.Symbols
{
    public class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new("?");
        
        public static readonly TypeSymbol Object = new("object");
        public static readonly TypeSymbol Bool = new("bool");
        public static readonly TypeSymbol Int = new("int");
        public static readonly TypeSymbol String = new("string");
        public static readonly TypeSymbol Void = new("void");
        
        private TypeSymbol(string name) : base(name)
        {

        }

        public override SymbolKind Kind => SymbolKind.Type;
    }
}