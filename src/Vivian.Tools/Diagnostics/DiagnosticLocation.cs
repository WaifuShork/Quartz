using System;

namespace Vivian.Tools.Diagnostics
{
    public class DiagnosticLocation
    {
        public DiagnosticLocation(Uri uri, Range range)
        {
            Uri = uri;
            Range = range;
        }

        public Uri Uri { get; set; }
        public Range Range { get; set; }
    }
}