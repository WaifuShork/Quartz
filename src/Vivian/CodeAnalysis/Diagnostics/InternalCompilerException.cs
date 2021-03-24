using System;
using Vivian.CodeAnalysis.Text;

namespace Vivian.CodeAnalysis
{
    internal class InternalCompilerException : Exception
    {
        public InternalCompilerException(string message)
        {
            Console.Error.WriteLine(message);
            // throw new Exception(message);
        }
    }
}