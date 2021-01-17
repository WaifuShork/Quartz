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
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: vc <source-paths>.");
                return 1;
            }

            var paths = GetFilePaths(args);
            var syntaxTrees = new List<SyntaxTree>();
            var hasErrors = false;

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"error: file '{path}' does not exist.");
                    hasErrors = true;
                    continue;
                }
                var syntaxTree = SyntaxTree.Load(path);

                syntaxTrees.Add(syntaxTree);
            }

            if (hasErrors)
                return 1;
            
            var compilation = Compilation.Create(syntaxTrees.ToArray());
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            if (!result.Diagnostics.Any())
            {
                if (result.Value != null)
                    Console.WriteLine(result.Value);
            }
            else
            {
                Console.Error.WriteDiagnostics(result.Diagnostics);
                return 1;
            }

            return 0;
        }

        private static IEnumerable<string> GetFilePaths(IEnumerable<string> paths)
        {
            var result = new SortedSet<string>();
            
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    result.UnionWith(Directory.EnumerateFiles(path, "*.viv", SearchOption.AllDirectories));
                }
                else
                {
                    result.Add(path);
                }
            }

            return result;
        }
    }
}