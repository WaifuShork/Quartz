using System.IO;

namespace Vivian.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
        
        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                this.WriteTo(writer);
                return writer.ToString();
            }
        }
    }
}