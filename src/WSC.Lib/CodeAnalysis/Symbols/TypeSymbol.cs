namespace wsc.CodeAnalysis.Symbols
{
    public class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new TypeSymbol("?");
        public static readonly TypeSymbol Bool = new TypeSymbol("int");
        public static readonly TypeSymbol Int = new TypeSymbol("bool");
        public static readonly TypeSymbol String = new TypeSymbol("string");
        
        private TypeSymbol(string name) : base(name)
        {
            
        }
        
        public override SymbolKind Kind => SymbolKind.Type;
    }
    
}