using System.Collections.Immutable;
using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundProgram
    {
        public BoundProgram(BoundProgram previous, 
                            ImmutableArray<Diagnostic> diagnostics, 
                            ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, 
                            BoundBlockStatement statement)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Functions = functions;
            Statement = statement;
        }

        public BoundProgram Previous { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
        public BoundBlockStatement Statement { get; }
    }
}