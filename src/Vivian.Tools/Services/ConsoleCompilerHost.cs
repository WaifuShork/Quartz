using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Vivian.CodeAnalysis;
using Vivian.IO;

namespace Vivian.Tools.Services
{
    internal class ConsoleCompilerHost : IHost
    {
        public int Errors;
        public int Warnings;

        public void Dispose() { }
        
        public void PublishDiagnostics(IEnumerable<Diagnostic> diag, CancellationToken cancellationToken)
        {
            var diagnostics = diag as Diagnostic[] ?? diag.ToArray();
            Errors += diagnostics.Count(d => d.IsError);
            Warnings += diagnostics.Count(d => d.IsWarning);
            
            Console.Error.WriteDiagnostics(diagnostics);
        }

        public void RequestShutdown()
        {
            Console.Error.WriteLine("Compiler Crashed");
            Environment.Exit(1);
        }
    }
}