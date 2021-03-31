using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vivian.CodeAnalysis;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.Tools.Services
{
    public class Server
    {
        private readonly IHost _host;
        private bool _shutDownRequested;
        private bool _isExiting;

        public Server(IHost host)
        {
            _host = host;
        }

        public Task Initialize(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<Compilation> Validate(string source, string sourcePath, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var tree = SyntaxTree.Parse(source, sourcePath);
                
                cancellationToken.ThrowIfCancellationRequested();

                if (tree.Diagnostics.Length > 0)
                {
                    _host.PublishDiagnostics(tree.Diagnostics, cancellationToken);
                }

                var program = Compilation.Create(tree);
                cancellationToken.ThrowIfCancellationRequested();
                
                var diagnostics = program.Validate();
                cancellationToken.ThrowIfCancellationRequested();
                
                if (diagnostics.Length > 0)
                {
                    _host.PublishDiagnostics(diagnostics, cancellationToken);
                }

                return program;
                
            }, cancellationToken);
        }
        
        public async Task<IEnumerable<SyntaxTree>> Parse(IList<string> sourcePaths,
                                                         CancellationToken cancellationToken = default)
        {
            var syntaxTrees = new ConcurrentBag<SyntaxTree>();

            // Debugger.Launch();

            // Use ParallelOptions instance to store the CancellationToken
            var parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            // If we are compiling a small amount of files, favor sequential processing.
            if (sourcePaths.Count < Environment.ProcessorCount)
            {
                foreach (var path in sourcePaths)
                {
                    var syntaxTree = SyntaxTree.Load(path);
                    syntaxTrees.Add(syntaxTree);
                }
            }
            else
            {
                // Load files in parallel
                try
                {
                    Parallel.ForEach(sourcePaths, parallelOptions, (path) =>
                    {
                        var syntaxTree = SyntaxTree.Load(path);
                        syntaxTrees.Add(syntaxTree);
                        parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                    });
                }
                catch (OperationCanceledException e)
                {
                    await Console.Error.WriteLineAsync("Compilation cancelled.");
                    await Console.Error.WriteLineAsync(e.InnerException?.ToString());

                    throw;
                }
            }

            foreach (var tree in syntaxTrees)
            {
                if (tree.Diagnostics.Length > 0)
                {
                     _host.PublishDiagnostics(tree.Diagnostics, cancellationToken);
                }
            }

            return syntaxTrees;
        }

        public bool EmitBinary(IEnumerable<SyntaxTree> syntaxTrees, string moduleName, string[] referencePaths, string outputPath)
        {
            var compilation = Compilation.Create(syntaxTrees.ToArray());

            if (compilation == null!)
            {
                return false;
            }

            var diagnostics = compilation.Emit(moduleName, referencePaths, outputPath);

            if (diagnostics.Length > 0)
            {
                _host.PublishDiagnostics(diagnostics);
            }

            return true;
        }
        
        public Task Shutdown(CancellationToken cancellationToken = default)
        {
            _shutDownRequested = true;
            return Task.CompletedTask;
        }
        
        public int Exit()
        {
            if (!_shutDownRequested || _isExiting)
            {
                return 1;
            }

            _isExiting = true;

            return 0;
        }
    }
}