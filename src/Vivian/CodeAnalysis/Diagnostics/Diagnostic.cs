using System;
using System.IO;
using Vivian.CodeAnalysis.Text;

namespace Vivian.CodeAnalysis
{
    public class Diagnostic
    {
        private Diagnostic(bool isError, TextLocation location, string message)
        {
            IsError = isError;
            Location = location;
            Message = message;
            IsWarning = !IsError;
        }

        public bool IsError { get; }
        public TextLocation Location { get; }
        public string Message { get; }
        public bool IsWarning { get; }

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