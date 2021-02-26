using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vivian.CodeAnalysis;
using Vivian.CodeAnalysis.Syntax;
using Vivian.CodeAnalysis.Text;

namespace Vivian.IO
{
    public static class TextWriterExtensions
        {
            private static bool IsConsole(this TextWriter writer)
            {
                if (writer == Console.Out)
                    return !Console.IsOutputRedirected;
    
                if (writer == Console.Error)
                    return !Console.IsErrorRedirected && !Console.IsOutputRedirected; // Color codes are always output to Console.Out
    
                if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole())
                    return true;
    
                return false;
            }
    
            private static void SetForeground(this TextWriter writer, ConsoleColor color)
            {
                if (writer.IsConsole())
                    Console.ForegroundColor = color;
            }
    
            private static void ResetColor(this TextWriter writer)
            {
                if (writer.IsConsole())
                    Console.ResetColor();
            }
    
            public static void WriteBuildSummary(this TextWriter writer, bool success, int errors, int warnings)
            {
                writer.SetForeground(success ? ConsoleColor.Green : ConsoleColor.DarkRed);
                writer.WriteLine($"Build {(success ? "Succeeded" : "Failed" )}.");
                writer.WriteLine($"{warnings,5} Warning(s)");
                writer.WriteLine($"{errors,5} Error(s)");
            }
    
            public static void WriteKeyword(this TextWriter writer, SyntaxKind kind)
            {
                var text = SyntaxFacts.GetText(kind);
                Debug.Assert(kind.IsKeyword() && text != null);
    
                writer.WriteKeyword(text);
            }
    
            public static void WriteKeyword(this TextWriter writer, string text)
            {
                writer.SetForeground(ConsoleColor.Blue);
                writer.Write(text);
                writer.ResetColor();
            }
    
            public static void WriteIdentifier(this TextWriter writer, string text)
            {
                writer.SetForeground(ConsoleColor.DarkYellow);
                writer.Write(text);
                writer.ResetColor();
            }
    
            public static void WriteNumber(this TextWriter writer, string text)
            {
                writer.SetForeground(ConsoleColor.Cyan);
                writer.Write(text);
                writer.ResetColor();
            }
    
            public static void WriteString(this TextWriter writer, string text)
            {
                writer.SetForeground(ConsoleColor.Magenta);
                writer.Write(text);
                writer.ResetColor();
            }
    
            public static void WriteSpace(this TextWriter writer)
            {
                writer.WritePunctuation(" ");
            }
    
            public static void WriteComment(this TextWriter writer, string text)
            {
                writer.SetForeground(ConsoleColor.DarkGreen);
                writer.Write("// ");
                writer.Write(text);
                writer.ResetColor();
            }
    
            public static void WritePunctuation(this TextWriter writer, SyntaxKind kind)
            {
                var text = SyntaxFacts.GetText(kind);
                Debug.Assert(text != null);
    
                writer.WritePunctuation(text);
            }
    
            public static void WritePunctuation(this TextWriter writer, string text)
            {
                writer.SetForeground(ConsoleColor.DarkGray);
                writer.Write(text);
                writer.ResetColor();
            }
    
            public static void WriteDiagnostics(this TextWriter writer, IEnumerable<IDiagnostic> diagnostics)
            {
                foreach (var diagnostic in diagnostics.Where(d => d.DiagnosticLocation.Uri == null))
                {
                    var messageColor = diagnostic.IsWarning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
                    writer.SetForeground(messageColor);
                    writer.WriteLine(diagnostic.Message);
                    writer.ResetColor();
                }
    
                foreach (var diagnostic in diagnostics.Where(d => d.DiagnosticLocation.Uri != null)
                                                      .OrderBy(d => d.DiagnosticLocation.Uri.AbsolutePath)
                                                      .ThenBy(d => d.DiagnosticLocation.Range.Start)
                                                      .ThenBy(d => d.DiagnosticLocation.Range.End))
                {
                    var text = diagnostic.ContextSourceSnippet;
                    var fileName = diagnostic.DiagnosticLocation.Uri.LocalPath;
                    var startLine = diagnostic.DiagnosticLocation.Range.Start.Line;
                    var startCharacter = diagnostic.DiagnosticLocation.Range.Start.Character;
                    var endLine = diagnostic.DiagnosticLocation.Range.End.Line;
                    var endCharacter = diagnostic.DiagnosticLocation.Range.End.Character;
    
                    writer.WriteLine();
    
                    var messageColor = diagnostic.IsWarning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
                    writer.SetForeground(messageColor);
                    writer.Write($"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): ");
                    writer.WriteLine(diagnostic);
                    writer.ResetColor();
    
                    if (text != null) {
                        var lines = text.Split('\n', StringSplitOptions.None);
                        string prefix;
                        string error;
                        string suffix;
    
                        // Simple case, there is just 1 line
                        if (lines.Length == 1) {
                            prefix = lines[0][0..diagnostic.DiagnosticLocation.Range.Start.Character];
                            suffix = lines[0][diagnostic.DiagnosticLocation.Range.End.Character..^0];
                            error = lines[0][diagnostic.DiagnosticLocation.Range.Start.Character..diagnostic.DiagnosticLocation.Range.End.Character];
                        }
                        else {
                            prefix = lines[0][0..diagnostic.DiagnosticLocation.Range.Start.Character];
                            suffix = lines[^1][diagnostic.DiagnosticLocation.Range.End.Character..^0];
                            error = diagnostic.TargetSourceSnippet ?? string.Empty;
                        }
    
                        writer.Write("    ");
                        writer.Write(prefix);
                        writer.SetForeground(messageColor);
                        writer.Write(error);
                        writer.ResetColor();
    
                        writer.Write(suffix);
    
                        writer.WriteLine();
                    }
                }
    
                writer.WriteLine();
            }
        }
}