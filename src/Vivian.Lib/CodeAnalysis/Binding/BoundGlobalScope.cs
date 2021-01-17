using System.Collections.Immutable;
using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(BoundGlobalScope previous, 
                                ImmutableArray<Diagnostic> diagnostics,
                                FunctionSymbol mainFunction,
                                ImmutableArray<FunctionSymbol> functions, 
                                ImmutableArray<VariableSymbol> variables,
                                ImmutableArray<BoundStatement> statements)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            MainFunction = mainFunction;
            Functions = functions;
            Variables = variables;
            Statements = statements;
        }
        
        public BoundGlobalScope Previous { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public FunctionSymbol MainFunction { get; }
        public ImmutableArray<FunctionSymbol> Functions { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}