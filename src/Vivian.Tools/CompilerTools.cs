using System;
using System.Collections.Generic;
using System.Linq;

using Vivian.CodeAnalysis;
using Vivian.CodeAnalysis.Syntax;
using Vivian.IO;

namespace Vivian.Tools.Services
{
    public class CompilerTools
    {
        public int Errors { get; private set; }
        public int Warnings { get; private set; }
        
        public void EmitBinary(IEnumerable<SyntaxTree> syntaxTrees, string moduleName, string[] referencePaths, string outputPath)
        {
            var compilation = Compilation.Create(syntaxTrees.ToArray());

            if (compilation == null!)
            {
                return;
            }

            var diagnostics = compilation.Emit(moduleName, referencePaths, outputPath);

            if (diagnostics.Length > 0)
            {
                PublishDiagnostics(diagnostics);
            }
        }

        private void PublishDiagnostics(IEnumerable<Diagnostic> diag)
        {
            if (diag is not Diagnostic[] diagnostics)
            {
                diagnostics = diag.ToArray();
            }

            Errors += diagnostics.Count(d => d.IsError);
            Warnings += diagnostics.Count(d => d.IsWarning);
            
            Console.Error.WriteDiagnostics(diagnostics);
        }
    }
}