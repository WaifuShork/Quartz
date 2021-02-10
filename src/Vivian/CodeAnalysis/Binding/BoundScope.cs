using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        //private Dictionary<string, VariableSymbol> _globals;
        //private Dictionary<string, FunctionSymbol> _functions;
        private Dictionary<string, Symbol> _symbols;
        
        
        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }
        public BoundScope Parent { get; }

        public bool TryDeclareVariable(VariableSymbol variable)
            => TryDeclareSymbol(variable);

        public bool TryDeclareFunction(FunctionSymbol function)
            => TryDeclareSymbol(function);

        private bool TryDeclareSymbol<TSymbol>(TSymbol symbol) where TSymbol : Symbol
        {
            if (_symbols == null)
                _symbols = new Dictionary<string, Symbol>();
            else if (_symbols.ContainsKey(symbol.Name))
                return false;

            _symbols.Add(symbol.Name, symbol);
            return true;
        }
        
        /*public bool TryLookupVariable(string name, out VariableSymbol variable)
            => TryLookupSymbol(name, out variable);
        
        public bool TryLookupFunction(string name, out FunctionSymbol function)
            => TryLookupSymbol(name, out function);

        private bool TryLookupSymbol<TSymbol>(string name, out TSymbol symbol) where TSymbol : Symbol
        {
            symbol = null;
            
            if (_symbols != null && _symbols.TryGetValue(name, out var declaredSymbol))
            {
                if (declaredSymbol is TSymbol matchingSymbol)
                {
                    symbol = matchingSymbol;
                    return true;
                }
                return false;
            }

            if (Parent == null)
                return false;

            return Parent.TryLookupSymbol(name, out symbol);
        }*/

        public Symbol TryLookupSymbol(string name)
        {
            if (_symbols != null && _symbols.TryGetValue(name, out var symbol))
                return symbol;

            return Parent?.TryLookupSymbol(name);
        }
        

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
            => GetDeclaredSymbols<VariableSymbol>();

        
        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() 
            => GetDeclaredSymbols<FunctionSymbol>();

        private ImmutableArray<TSymbol> GetDeclaredSymbols<TSymbol>() where TSymbol : Symbol
        {
            if (_symbols == null)
                return ImmutableArray<TSymbol>.Empty;

            return _symbols.Values.OfType<TSymbol>().ToImmutableArray();
        }
        
        /*public bool TryDeclareVariable(VariableSymbol variable)
        /
            if (_globals == null)
                _globals = new Dictionary<string, VariableSymbol>();
            
            if (_globals.ContainsKey(variable.Name))
                return false;
            
            _globals.Add(variable.Name, variable);
            return true;
        }
        
        public bool TryLookupVariable(string name, out VariableSymbol variable)
        {
            variable = null;
            if (_globals != null && _globals.TryGetValue(name, out variable))
                return true;

            if (Parent == null)
                return false;
            
            return Parent.TryLookupVariable(name, out variable);
        }

        public bool TryDeclareFunction(FunctionSymbol function)
        {
            if (_functions == null)
                _functions = new Dictionary<string, FunctionSymbol>();
            
            if (_functions.ContainsKey(function.Name))
                return false;
            
            _functions.Add(function.Name, function);
            return true;
        }
        
        public bool TryLookupFunction(string name, out FunctionSymbol function)
        {
            function = null;
            if (_functions != null && _functions.TryGetValue(name, out function))
                return true;

            if (Parent == null)
                return false;

            return Parent.TryLookupFunction(name, out function);
        }
        
        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        {
            if (_globals == null)
                return ImmutableArray<VariableSymbol>.Empty;
            
            return _globals.Values.ToImmutableArray();
        }
        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
        {
            if (_functions == null)
                return ImmutableArray<FunctionSymbol>.Empty;
            
            return _functions.Values.ToImmutableArray();
        }        */

    }
}