using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Symbols
{
    [Flags]
    public enum TypeSymbolCaps
    {
        None             = 0x0000,

        Array            = 0x0001,
        Pointer          = 0x0002,
        FixedPoint       = 0x0004,
        Signed           = 0x0008,
        FloatingPointBin = 0x0010,
        FloatingPointDec = 0x0020,
        _Comparable      = 0x0040,

        FloatingPoint    = FloatingPointBin | FloatingPointDec,
        Arithmetic       = FixedPoint | FloatingPoint,
        Comparable       = _Comparable | Arithmetic,

        All              = 0x00ff,

        Bit8             = 0x0100,
        Bit16            = 0x0200,
        Bit32            = 0x0300,
        Bit64            = 0x0400,
        Bit128           = 0x0500,
        Bits             = Bit8 | Bit16 | Bit32 | Bit64 | Bit128
    }

    public class TypeSymbol : Symbol
    {
        public static ImmutableDictionary<string, TypeSymbol> NameToType = ImmutableDictionary.Create<string, TypeSymbol>();

        public static readonly TypeSymbol Error = new TypeSymbol("?", TypeSymbolCaps.None, null);

        public static readonly TypeSymbol Object = new TypeSymbol("object", TypeSymbolCaps.Pointer, typeof(long)); // long for pointer

        public static readonly TypeSymbol Bool = new TypeSymbol("bool", TypeSymbolCaps.FixedPoint | TypeSymbolCaps.Bit8, typeof(byte)); // TODO: add
        public static readonly TypeSymbol Byte = new TypeSymbol("byte", TypeSymbolCaps.FixedPoint | TypeSymbolCaps.Bit8, typeof(byte)); // TODO: add
        public static readonly TypeSymbol Char = new TypeSymbol("char", TypeSymbolCaps.FixedPoint | TypeSymbolCaps.Bit16, typeof(char)); // TODO: add
        public static readonly TypeSymbol Short = new TypeSymbol("short", TypeSymbolCaps.FixedPoint | TypeSymbolCaps.Signed | TypeSymbolCaps.Bit16, typeof(short)); // TODO: add
        public static readonly TypeSymbol Int = new TypeSymbol("int", TypeSymbolCaps.FixedPoint | TypeSymbolCaps.Signed | TypeSymbolCaps.Bit32, typeof(int));
        public static readonly TypeSymbol Long = new TypeSymbol("long", TypeSymbolCaps.FixedPoint | TypeSymbolCaps.Signed | TypeSymbolCaps.Bit64, typeof(long)); // TODO: add
        //public static readonly TypeSymbol LongLong = new TypeSymbol("long long", TypeSymbolCaps.FixedPoint | TypeSymbolCaps.Signed | TypeSymbolCaps.Bit128); // TODO: maybe add

        //public static readonly TypeSymbol Half = new TypeSymbol("half", TypeSymbolCaps.FloatingPointBin | TypeSymbolCaps.Bit16); // TODO: maybe add
        public static readonly TypeSymbol Float = new TypeSymbol("float", TypeSymbolCaps.FloatingPointBin | TypeSymbolCaps.Bit32, typeof(float)); // TODO: add
        public static readonly TypeSymbol Double = new TypeSymbol("double", TypeSymbolCaps.FloatingPointBin | TypeSymbolCaps.Bit64, typeof(double)); // TODO: add
        public static readonly TypeSymbol Decimal = new TypeSymbol("decimal", TypeSymbolCaps.FloatingPointDec | TypeSymbolCaps.Bit128, typeof(decimal)); // TODO: add

        public static readonly TypeSymbol String = new TypeSymbol("string", TypeSymbolCaps.Array | TypeSymbolCaps._Comparable | TypeSymbolCaps.Bit16, typeof(long));

        public static readonly TypeSymbol Void = new TypeSymbol("void", TypeSymbolCaps.Pointer, typeof(long)); // TODO: probably delete this and replace with a dedicated pointer type?

        private TypeSymbol(string name, TypeSymbolCaps caps, Type container) : base(name)
        {
            Caps = caps;
            Container = container;
            NameToType = NameToType.Add(Name, this);
        }

        private static readonly new Dictionary<ValueTuple<TypeSymbol, TypeSymbol>, TypeSymbol> PromotionRules = new ()
        {
            { (Bool, Bool), Bool },
            { (Bool, Byte), Int },
            { (Bool, Char), Int },
            { (Bool, Short), Int },
            { (Bool, Int), Int },
            { (Bool, Long ), Long },
            { (Bool, Float), Float },
            { (Bool, Double), Double },
            { (Bool, Decimal), Decimal },
            { (Byte, Byte), Int },
            { (Byte, Char), Int },
            { (Byte, Short), Int },
            { (Byte, Int), Int },
            { (Byte, Long), Long },
            { (Byte, Float), Float },
            { (Byte, Double), Double },
            { (Byte, Decimal), Decimal },
            { (Char, Char), Int },
            { (Char, Short), Int },
            { (Char, Int), Int },
            { (Char, Long), Long },
            { (Char, Float), Float },
            { (Char, Double), Double },
            { (Char, Decimal), Decimal },
            { (Short, Short), Int },
            { (Short, Int), Int },
            { (Short, Long), Long },
            { (Short, Float), Float },
            { (Short, Double), Double },
            { (Short, Decimal), Decimal },
            { (Int, Int), Int },
            { (Int, Long), Long },
            { (Int, Float), Float },
            { (Int, Double), Double },
            { (Int, Decimal), Decimal },
            { (Long, Long), Long },
            { (Long, Float), Float },
            { (Long, Double), Double },
            { (Long, Decimal), Decimal },
            { (Float, Float), Float },
            { (Float, Double), Double }
        };

        public TypeSymbolCaps Caps { get; }
        public Type Container { get; }

        public override SymbolKind Kind => SymbolKind.Type;

        public static TypeSymbol Promotion(TypeSymbol typeA, TypeSymbol typeB)
        {
            if (PromotionRules.TryGetValue((typeA, typeB), out TypeSymbol promotion1))
            {
                return promotion1;
            }
            else if (PromotionRules.TryGetValue((typeB, typeA), out TypeSymbol promotion2))
            {
                return promotion2;
            }
            throw new Exception($"Types {typeA} and {typeB} can't be implicitly promoted");
        }
    }

    public sealed class TypeComparator
    {
        private TypeSymbol _sym = TypeSymbol.Error;
        private TypeSymbolCaps _caps = TypeSymbolCaps.None;

        public TypeComparator (TypeSymbol symbol)
        {
            _sym = symbol;
        }

        public TypeComparator (TypeSymbolCaps symbolCaps)
        {
            _caps = symbolCaps;
        }

        public bool Test(TypeSymbol test)
        {
            if (_sym == TypeSymbol.Error && _caps != TypeSymbolCaps.None)
                return _Test(test.Caps);
            else if (_sym != TypeSymbol.Error && _caps == TypeSymbolCaps.None)
                return _Test(test);
            else
                throw new Exception($"Invalid TypeComparator: \n\tType: {_sym}\n\tCaps: {_caps}");
        }

        private bool _Test(TypeSymbol test) => test == _sym;
        private bool _Test(TypeSymbolCaps test) => (test & _caps) != TypeSymbolCaps.None;
    }
}