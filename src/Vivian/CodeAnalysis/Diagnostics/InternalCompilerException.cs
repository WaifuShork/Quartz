using System;
using Vivian.CodeAnalysis.Text;

namespace Vivian.CodeAnalysis
{
    internal class InternalCompilerException : Exception
    {
        public InternalCompilerException(string message) : base(message) { }
    }
}