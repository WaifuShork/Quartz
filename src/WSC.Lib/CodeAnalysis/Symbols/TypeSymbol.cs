namespace wsc.CodeAnalysis.Symbols
{
    public class TypeSymbol : Symbol
    {
        internal TypeSymbol(string name) : base(name)
        {
            
        }
        
        public override SymbolKind Kind => SymbolKind.Variable;

    }
}