using wsc.CodeAnalysis.Text;

namespace wsc.CodeAnalysis
{
    /// <summary>
    /// Represents the current diagnostics in the compiler.
    /// </summary>
    public sealed class Diagnostic
    {
        public Diagnostic(TextSpan span, string message)
        {
            Span = span;
            Message = message;
        }
        
        /// <summary>
        /// Represents the current TextSpan in the diagnostic reporting.
        /// </summary>
        public TextSpan Span { get; }
        
        /// <summary>
        /// Represents the Diagnostic message to be reported.
        /// </summary>
        public string Message { get; }

        public override string ToString() => Message;
    }
}