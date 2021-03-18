using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Vivian.CodeAnalysis
{
    public static class DiagnosticExtensions
    {
        public static bool HasWarnings(this ImmutableArray<Diagnostic> diagnostics)
        {
            return diagnostics.Any(d => d.IsWarning);
        }

        public static bool HasErrors(this IEnumerable<Diagnostic> diagnostics)
        {
            return diagnostics.Any(d => d.IsError);
        }
    }
}