using System.Collections.Immutable;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Symbols
{
    public sealed class ClassSymbol : TypeSymbol
    {
        internal ClassSymbol(string name, ImmutableArray<ParameterSymbol> ctorParameters, ImmutableArray<VariableSymbol> members, ClassDeclarationSyntax? declaration = null) 
            : base(name, null)
        {
            Declaration = declaration;
            CtorParameters = ctorParameters;
            Members = members;
        }

        public override SymbolKind Kind => SymbolKind.Class;

        public ClassDeclarationSyntax? Declaration { get; }
        public ImmutableArray<ParameterSymbol> CtorParameters { get; }
        public ImmutableArray<VariableSymbol> Members { get; }
    }
}