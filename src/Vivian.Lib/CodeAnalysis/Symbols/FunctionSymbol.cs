using System.Collections.Immutable;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        internal FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type, FunctionDeclarationSyntax declaration = null) : base(name)
        {
            Parameters = parameters;
            Type = type;
            Declaration = declaration;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol Type { get; }
        public FunctionDeclarationSyntax Declaration { get; }
    }
}