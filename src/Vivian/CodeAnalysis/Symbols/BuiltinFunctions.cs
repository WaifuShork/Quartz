using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Vivian.CodeAnalysis.Symbols
{
    /// <summary>
    /// Represents all the built-in functions that the compiler has
    /// </summary>
    internal static class BuiltinFunctions
    {
        // I/O Methods
        public static readonly FunctionSymbol WriteLine = new("WriteLine", ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.Object, 0)), TypeSymbol.Void);
        public static readonly FunctionSymbol Write = new("Write", ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.Object, 0)), TypeSymbol.Void);
        
        public static readonly FunctionSymbol ReadLine = new("ReadLine", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);
        public static readonly FunctionSymbol ReadKey = new("ReadKey", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);
        
        public static readonly FunctionSymbol ReadAllText = new("ReadAllText", ImmutableArray.Create(new ParameterSymbol("path", TypeSymbol.String, 0)), TypeSymbol.String);
        public static readonly FunctionSymbol WriteAllText =
            new("WriteAllText", ImmutableArray.Create(
                new ParameterSymbol("path", TypeSymbol.String, 0), // arg 1
                new ParameterSymbol("contents", TypeSymbol.String, 1)), // arg 2
                TypeSymbol.Void); // return type

        // Random.Next because why not
        public static readonly FunctionSymbol Rnd = new("rnd", ImmutableArray.Create(new ParameterSymbol("max", TypeSymbol.Int32, 0)), TypeSymbol.Int32);
        
        // Gets all the built in functions for binding, lowering, and emitting
        internal static IEnumerable<FunctionSymbol> GetAll()
            => typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                                       .Where(f => f.FieldType == typeof(FunctionSymbol))
                                       .Select(f => (FunctionSymbol) f.GetValue(null)!);
        
    }
}