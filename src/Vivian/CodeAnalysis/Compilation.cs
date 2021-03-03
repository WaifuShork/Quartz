using ReflectionBindingFlags = System.Reflection.BindingFlags;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

using Vivian.CodeAnalysis.Binding;
using Vivian.CodeAnalysis.Emit;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope? _globalScope;

        private Compilation(params SyntaxTree[] syntaxTrees)
        {
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public static Compilation Create(params SyntaxTree[] syntaxTrees)
        {
            return new(syntaxTrees);
        }

        public Compilation? Previous { get; }
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
        
        public FunctionSymbol? MainFunction => GlobalScope.MainFunction;
        public ImmutableArray<FunctionSymbol> Functions => GlobalScope.Functions;
        public ImmutableArray<VariableSymbol> Variables => GlobalScope.Variables;
        public ImmutableArray<StructSymbol> Structs => GlobalScope.Structs;

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public IEnumerable<Symbol> GetSymbols()
        {
            var submission = this;
            var seenSymbolNames = new HashSet<string>();

            var builtinFunctions = BuiltinFunctions.GetAll().ToList();

            while (submission != null)
            {
                foreach (var @struct in submission.Structs.Where(@struct => seenSymbolNames.Add(@struct.Name)))
                {
                    yield return @struct;
                }

                foreach (var function in submission.Functions.Where(function => seenSymbolNames.Add(function.Name)))
                {
                    yield return function;
                }

                foreach (var variable in submission.Variables.Where(variable => seenSymbolNames.Add(variable.Name)))
                {
                    yield return variable;
                }

                foreach (var builtin in builtinFunctions.Where(builtin => seenSymbolNames.Add(builtin.Name)))
                {
                    yield return builtin;
                }

                submission = submission.Previous;
            }
        }

        private BoundProgram GetProgram()
        {
            var previous = Previous?.GetProgram();
            return Binder.BindProgram(previous, GlobalScope);
        }

        public ImmutableArray<Diagnostic> Validate()
        {
            var program = GetProgram();
            return program.Diagnostics;
        }

        // TODO: References should be part of the compilation, not arguments for Emit
        public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, string outputPath)
        {
            var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);
            var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();

            if (diagnostics.HasErrors())
                return diagnostics;

            var program = GetProgram();

            return Emitter.Emit(program, moduleName, references, outputPath);
        }
    }
}