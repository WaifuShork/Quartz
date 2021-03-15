using System.Collections.Immutable;

using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        internal FunctionSymbol(string name,
                                ImmutableArray<ParameterSymbol> parameters,
                                TypeSymbol returnType,
                                FunctionDeclarationSyntax? declaration = null,
                                FunctionSymbol? overloadFor = null,
                                ClassSymbol? receiver = null) 
                                : base(name)
        {
            Parameters = parameters;
            ReturnType = returnType;
            Declaration = declaration;
            OverloadFor = overloadFor;
            Receiver = receiver;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public FunctionDeclarationSyntax? Declaration { get; }
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
        public FunctionSymbol? OverloadFor { get; }
        public ClassSymbol? Receiver { get; }
    }
}