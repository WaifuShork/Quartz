using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace wsc.CodeAnalysis
{
    /// <summary>
    /// Represents a basic EvaluationResult given then current Diagnostics.
    /// </summary>
    public sealed class EvaluationResult
    {
        /// <summary>
        /// Constructor for EvaluationResult.
        /// </summary>
        /// <param name="diagnostics"></param>
        /// <param name="value"></param>
        public EvaluationResult(ImmutableArray<Diagnostic> diagnostics, object value)
        {
            Diagnostics = diagnostics;

            Value = value;
        }
        
        /// <summary>
        /// Represents an ImmutableArray of the current Diagnostics.
        /// </summary>
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// Represents the value for the EvaluationResult that's been produced.
        /// </summary>
        public object Value { get; }
    }
}