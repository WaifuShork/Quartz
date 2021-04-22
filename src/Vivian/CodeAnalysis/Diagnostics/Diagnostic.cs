using Vivian.CodeAnalysis.Text;

namespace Vivian.CodeAnalysis
{
    public class Diagnostic
    {
        private Diagnostic(bool isError, TextLocation location, string message)
        {
            IsError = isError;
            IsWarning = !IsError;

            Location = location;
            Message = message;
        }

        public bool IsError { get; }
        public bool IsWarning { get; }
        
        public TextLocation Location { get; }
        public string Message { get; }
        
        public string? TargetSourceSnippet => Location.Text.ToString(Location.Span.Start, Location.Span.End - Location.Span.Start);

        public override string ToString() => Message;

        public static Diagnostic Error(TextLocation location, string message)
        {
            return new(isError: true, location, message);
        }

        public static Diagnostic Warning(TextLocation location, string message)
        {
            return new(isError: false, location, message);
        }
    }
}