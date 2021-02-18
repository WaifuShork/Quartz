using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Options;

using Vivian.CodeAnalysis;
using Vivian.CodeAnalysis.Syntax;
using Vivian.IO;

namespace VivianCompiler
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var outputPath = (string) null!;
            var moduleName = (string) null!;
            
            var referencePaths = new List<string>();
            var sourcePaths = new List<string>();
            var helpRequested = false;
            
            var options = new OptionSet
            {
                "usage: vc <source-paths>. [options]",
                { "r=", "The {path} of an assembly to reference", v => referencePaths.Add(v) },
                { "o=", "The output {path} of the assembly to create", v => outputPath = v },
                { "m=", "The {name} of the module", v => moduleName = v },
                { "?|h|help", "Prints help", v => helpRequested = true},
                { "<>", v => sourcePaths.Add(v) },
            };

            options.Parse(args);

            if (helpRequested)
            {
                options.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            var paths = sourcePaths;

            if (paths.Count == 0)
            {
                Console.Error.WriteLine("error: need at least one source file");
                return 1;
            }

            if (outputPath == null)
            {
                outputPath = Path.ChangeExtension(sourcePaths[0], ".exe");
            }

            if (moduleName == null)
            {
                moduleName = Path.GetFileNameWithoutExtension(outputPath);
            }
            
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
            
            foreach (var path in referencePaths)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"error: file '{path}' does not exist.");
                    hasErrors = true;
                    continue;
                }
            }

            if (hasErrors)
                return 1;
            
            var compilation = Compilation.Create(syntaxTrees.ToArray());
            var diagnostics = compilation.Emit(moduleName, referencePaths.ToArray(), outputPath);

            if (diagnostics.Any())
            {
                Console.Error.WriteDiagnostics(diagnostics);
                return 1;
            }

            return 0;
        }
    }
}