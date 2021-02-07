using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Vivian.Generators
{
    [Generator]
    public class SyntaxNodeGetChildrenGenerator : ISourceGenerator
    {
        public void Initialize(InitializationContext context)
        {
        }
        
        public void Execute(SourceGeneratorContext  context)
        {
            SourceText sourceText;

            var compilation = (CSharpCompilation) context.Compilation;

            var types = GetAllTypes(compilation.Assembly);
            var syntaxNodeType = compilation.GetTypeByMetadataName("Vivian.CodeAnalysis.Syntax.SyntaxNode");
            var syntaxNodeTypes = types.Where(t => !t.IsAbstract && IsPartial(t) && IsDerivedFrom(t, syntaxNodeType));

            using (var stringWriter = new StringWriter())
            using (var indentedTextWriter = new IndentedTextWriter(stringWriter, "    "))
            {
                indentedTextWriter.WriteLine("namespace Vivian.CodeAnalysis.Syntax");
                indentedTextWriter.WriteLine("{");
                indentedTextWriter.Indent++;
                foreach (var type in syntaxNodeTypes)
                {
                    indentedTextWriter.WriteLine($"partial class {type.Name}");
                    indentedTextWriter.WriteLine("{");
                    indentedTextWriter.Indent++;
                    
                    // stuff
                    
                    indentedTextWriter.Indent--;
                    indentedTextWriter.WriteLine("}");
                }

                indentedTextWriter.Indent--;
                indentedTextWriter.WriteLine("}");

                indentedTextWriter.Flush();
                sourceText = SourceText.From(stringWriter.ToString(), Encoding.UTF8);
            }
                
            context.AddSource("Generated.cs", sourceText);

            var syntaxNodeFileName = syntaxNodeType.DeclaringSyntaxReferences.First().SyntaxTree.FilePath;

            var syntaxDirectory = Path.GetDirectoryName(syntaxNodeFileName);
            var fileName = Path.Combine(syntaxDirectory, "SyntaxNode_GetChildren.txt");

            using (var writer = new StreamWriter(fileName))
            {
                sourceText.Write(writer);
            }
        }

        private bool IsDerivedFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
        {
            while (type != null)
            {
                if (SymbolEqualityComparer.Default.Equals(type, baseType))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
        

        private bool IsPartial(INamedTypeSymbol type)
        {
            foreach (var declaration in type.DeclaringSyntaxReferences)
            {
                var syntax = declaration.GetSyntax();
                if (syntax is TypeDeclarationSyntax typeDeclaration)
                {
                    foreach (var modifier in typeDeclaration.Modifiers)
                    {
                        if (modifier.ValueText == "partial")
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public IReadOnlyList<INamedTypeSymbol> GetAllTypes(IAssemblySymbol symbol)
        {
            var result = new List<INamedTypeSymbol>();
            GetAllTypes(result, symbol.GlobalNamespace);
            return result;
        }

        private void GetAllTypes(List<INamedTypeSymbol> result, INamespaceOrTypeSymbol symbol)
        {
            if (symbol is INamedTypeSymbol type)
            {
                result.Add(type);
            }

            
            foreach (var child in symbol.GetMembers())
            {
                if (child is INamespaceOrTypeSymbol nsChild)
                {
                    GetAllTypes(result, nsChild);
                }
            }
        }
    }
}