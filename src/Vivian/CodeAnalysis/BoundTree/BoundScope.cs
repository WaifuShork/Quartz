using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, Symbol>? _symbols;

        public BoundScope(BoundScope? parent)
        {
            Parent = parent;
        }

        public BoundScope? Parent { get; }

        public bool TryDeclareVariable(VariableSymbol variable)
            => TryDeclareSymbol(variable);

        public bool TryDeclareFunction(FunctionSymbol function)
            => TryDeclareSymbol(function);

        public bool TryDeclareClass(ClassSymbol @class)
            => TryDeclareSymbol(@class);

        private bool TryDeclareSymbol<TSymbol>(TSymbol symbol) where TSymbol : Symbol
        {
            if (_symbols == null)
            {
                _symbols = new Dictionary<string, Symbol>();
            }
            else if (_symbols.ContainsKey(symbol.Name))
            {
                return false;
            }

            _symbols.Add(symbol.Name, symbol);

            return true;
        }

        public Symbol? TryLookupSymbol(string name)
        {
            if (_symbols != null && _symbols.TryGetValue(name, out var symbol))
            {
                return symbol;
            }

            return Parent?.TryLookupSymbol(name);
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
            => GetDeclaredSymbols<VariableSymbol>();

        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
            => GetDeclaredSymbols<FunctionSymbol>();

        public ImmutableArray<ClassSymbol> GetDeclaredStructs()
            => GetDeclaredSymbols<ClassSymbol>();

        private ImmutableArray<TSymbol> GetDeclaredSymbols<TSymbol>() where TSymbol : Symbol
        {
            if (_symbols == null)
            {
                return ImmutableArray<TSymbol>.Empty;
            }

            return _symbols.Values.OfType<TSymbol>().ToImmutableArray();
        }
    }
}