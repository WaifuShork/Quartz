﻿namespace Vivian.CodeAnalysis.Symbols
{
    public class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new TypeSymbol("?");
        public static readonly TypeSymbol Object = new TypeSymbol("object");
        public static readonly TypeSymbol Int = new TypeSymbol("int");
        public static readonly TypeSymbol String = new TypeSymbol("string");
        public static readonly TypeSymbol Void = new TypeSymbol("void");
        
        private TypeSymbol(string name) : base(name)
        {

        }

        public override SymbolKind Kind => SymbolKind.Type;
    }
}