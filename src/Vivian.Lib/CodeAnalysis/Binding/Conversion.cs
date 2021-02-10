using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class Conversion
    {
        public static readonly Conversion None = new Conversion(exists: false, isIdentity: false, isImplicit: false);
        public static readonly Conversion Identity = new Conversion(exists: true, isIdentity: true, isImplicit: true);
        public static readonly Conversion Implicit = new Conversion(exists: true, isIdentity: false, isImplicit: true);
        public static readonly Conversion Explicit = new Conversion(exists: true, isIdentity: false, isImplicit: false);

        private Conversion(bool exists, bool isIdentity, bool isImplicit)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }

        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => Exists && !IsImplicit;

        public static Conversion Classify(TypeSymbol from, TypeSymbol to)
        {
            if (from == to)
                return Conversion.Identity;

            if (from != TypeSymbol.Void && to == TypeSymbol.Object)
            {
                return Conversion.Implicit;
            }

            if (from == TypeSymbol.Object && to != TypeSymbol.Void)
            {
                return Conversion.Explicit;
            }

            if ((from.Caps & TypeSymbolCaps.Arithmetic) != TypeSymbolCaps.None)
            {
                if (to == TypeSymbol.String)
                    return Conversion.Explicit;
            }

            if (from == TypeSymbol.String)
            {
                if ((to.Caps & TypeSymbolCaps.Arithmetic) != TypeSymbolCaps.None)
                    return Conversion.Explicit;
            }

            if ((from.Caps & TypeSymbolCaps.FixedPoint) != TypeSymbolCaps.None)
            {
                if ((to.Caps & TypeSymbolCaps.FixedPoint) != TypeSymbolCaps.None)
                {
                    if ((from.Caps & TypeSymbolCaps.Bits) < (to.Caps & TypeSymbolCaps.Bits))
                        return Conversion.Implicit;
                    else
                        return Conversion.Explicit;
                }
                else if ((to.Caps & TypeSymbolCaps.FloatingPoint) != TypeSymbolCaps.None)
                    return Conversion.Implicit;
            }

            if ((from.Caps & TypeSymbolCaps.FloatingPointBin) != TypeSymbolCaps.None)
            {
                if ((to.Caps & TypeSymbolCaps.FloatingPoint) != TypeSymbolCaps.None)
                {
                    if ((from.Caps & TypeSymbolCaps.Bits) < (to.Caps & TypeSymbolCaps.Bits))
                    {
                        return Conversion.Implicit;
                    }
                    else
                    {
                        return Conversion.Explicit;
                    }
                }
                else if ((to.Caps & TypeSymbolCaps.FixedPoint) != TypeSymbolCaps.None)
                {
                    return Conversion.Explicit;
                }
            }


            return Conversion.None;
        }

        public static object Convert(TypeSymbol toType, object from)
        {
            return System.Convert.ChangeType(from, toType.Container);
        }

    }
}