using Vivian.CodeAnalysis.Authoring;
using Vivian.CodeAnalysis.Text;


namespace Vivian.CodeAnalysis.Authoring
{
    class ClassifiedSpan
    {
        public TextSpan Span { get; }
        public Classification Classification { get; }

        public ClassifiedSpan(TextSpan span, Classification classification)
        {
            Span = span;
            Classification = classification;
        }
    }
}