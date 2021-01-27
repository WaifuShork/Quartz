namespace Vivian.CodeAnalysis.Symbols
{
    public sealed class ParameterSymbol : LocalVariableSymbol
    {
        public ParameterSymbol(string name, TypeSymbol type) : base(name, isReadOnly: false, type)
        {
            
        }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}