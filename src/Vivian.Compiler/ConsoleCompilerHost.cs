using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Vivian.CodeAnalysis;
using Vivian.Host;
using Vivian.IO;

namespace VivianCompiler
{
    internal class ConsoleCompilerHost : IHost
    {
        public int Errors = 0;
        public int Warnings = 0;

        public void Dispose() { }
        
        public void PublishDiagnostics(IEnumerable<IDiagnostic> diag, CancellationToken cancellationToken)
        {
            var diagnostics = diag as IDiagnostic[] ?? diag.ToArray();
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