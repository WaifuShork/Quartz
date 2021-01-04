using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        internal FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameter, TypeSymbol type) : base(name)
        {
            Parameter = parameter;
            Type = type;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public ImmutableArray<ParameterSymbol> Parameter { get; }
        public TypeSymbol Type { get; }
    }
}