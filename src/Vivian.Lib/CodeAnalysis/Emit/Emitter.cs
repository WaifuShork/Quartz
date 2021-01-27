using System;
using System.Collections.Immutable;
using System.Linq;
using Mono.Cecil;
using Vivian.CodeAnalysis.Binding;

namespace Vivian.CodeAnalysis.Emit
{
    internal static class Emitter
    {
        public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
        {
            if (program.Diagnostics.Any())
            {
                return program.Diagnostics;
            }

            var result = new DiagnosticBag();
            
            var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
            var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);

            var typeDefinition = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed);
            assemblyDefinition.MainModule.Types.Add(typeDefinition);

           // var main = new MethodDefinition("Main", MethodAttributes.Static, voidType);
           assemblyDefinition.Write(outputPath);

           return result.ToImmutableArray();
        }
    }
}