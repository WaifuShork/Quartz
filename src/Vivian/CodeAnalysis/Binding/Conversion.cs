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
            // No cast necessary if both types are the same
            if (from == to)
                return Identity;

            if (from != TypeSymbol.Void && to == TypeSymbol.Object)
                return Implicit;

            // Casting to the same type but with more bits can be implicit
            if (from == TypeSymbol.Int8 && (to == TypeSymbol.Int16 || to == TypeSymbol.Int32 || to == TypeSymbol.Int64))
                return Implicit;

            if (from == TypeSymbol.Int16 && (to == TypeSymbol.Int32 || to == TypeSymbol.Int64))
                return Implicit;

            if (from == TypeSymbol.Int32 && to == TypeSymbol.Int64)
                return Implicit;

            if (from == TypeSymbol.UInt8 && (to == TypeSymbol.UInt16 || to == TypeSymbol.UInt32 || to == TypeSymbol.UInt64))
                return Implicit;

            if (from == TypeSymbol.UInt16 && (to == TypeSymbol.UInt32 || to == TypeSymbol.UInt64))
                return Implicit;

            if (from == TypeSymbol.UInt32 && to == TypeSymbol.UInt64)
                return Implicit;

            if ((from == TypeSymbol.Int8 || from == TypeSymbol.Int16 || from == TypeSymbol.Int32 || from == TypeSymbol.Int64 || from == TypeSymbol.UInt8 || from == TypeSymbol.UInt16 || from == TypeSymbol.UInt32 || from == TypeSymbol.UInt64)
                && (to == TypeSymbol.Float32 || to == TypeSymbol.Float64 || to == TypeSymbol.Decimal))
                return Implicit;

            if (from == TypeSymbol.Float32 && to == TypeSymbol.Float64)
                return Implicit;

            // Explicit casts
            if (from == TypeSymbol.Object && to != TypeSymbol.Void)
                return Explicit;

            if (from.IsNumeric && to.IsNumeric)
                return Explicit;

            if (from == TypeSymbol.Bool || from.IsNumeric)
                if (to == TypeSymbol.String)
                    return Explicit;

            if (from == TypeSymbol.String)
                if (to == TypeSymbol.Bool || to.IsNumeric)
                    return Explicit;

            return None;
        }
    }
}