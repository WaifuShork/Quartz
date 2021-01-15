using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Vivian.CodeAnalysis;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;
using Vivian.IO;

namespace VivianCompiler
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: vc <source-paths>.");
                return;
            }

            if (args.Length > 1)
            {
                Console.WriteLine("error: only one path support currently.");
                return;
            }

            var path = args.Single();

            if (!File.Exists(path))
            {
                Console.WriteLine($"error: file '{path}' does not exist.");
                return;
            }
            
            var syntaxTree = SyntaxTree.Load(path);

            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            if (!result.Diagnostics.Any())
            {
                if (result.Value != null)
                    Console.WriteLine(result.Value);
            }
            else
            {
                Console.Error.WriteDiagnostics(result.Diagnostics, syntaxTree);
            }
        }
    }
}