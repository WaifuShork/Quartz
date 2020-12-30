using System;

namespace wsc.CodeAnalysis
{
    /// <summary>
    /// Represents a basic variable.
    /// </summary>
    public sealed class VariableSymbol
    {
        internal VariableSymbol(string name, bool isReadOnly,Type type)
        {
            Name = name;
            IsReadOnly = isReadOnly;
            Type = type;
        }
        
        /// <summary>
        /// Represents the name of the variable.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Represents if the variable is readonly.
        /// </summary>
        public bool IsReadOnly { get; }
        
        /// <summary>
        /// Represents the type of the variable.
        /// </summary>
        public Type Type { get; }
    }
}