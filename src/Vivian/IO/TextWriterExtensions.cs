using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using System.Collections.Generic;

using Vivian.CodeAnalysis;
using Vivian.CodeAnalysis.Text;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.IO
{
    public static class TextWriterExtensions
    {
        private static bool IsConsole(this TextWriter writer)
        {
            if (writer == Console.Out)
            {
                return !Console.IsOutputRedirected;
            }

            if (writer == Console.Error)
            {
                return !Console.IsErrorRedirected && !Console.IsOutputRedirected; 
            }
            
            if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole())
            {
                return true;
            }

            return false;
        }

        private static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsole())
            {
                Console.ForegroundColor = color;
            }
        }

        private static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsole())
            {
                Console.ResetColor();
            }
        }

        // Async
        public static async Task WriteErrorAsync<T>(this TextWriter writer, T message)
        {
            await writer.WriteColorAsync(message, ConsoleColor.DarkRed);
        }

        public static async Task WriteSuccessAsync<T>(this TextWriter writer, T message)
        {
            await writer.WriteColorAsync(message, ConsoleColor.Green);
        }

        public static async Task WriteColorAsync<T>(this TextWriter writer, T message, ConsoleColor color = ConsoleColor.White, bool isNewLine = true)
        {
            if (message == null)
            {
                await writer.WriteLineAsync();
                return;
            }

            Console.ForegroundColor = color;
            if (isNewLine)
            {
                await writer.WriteLineAsync(message.ToString());
            }
            else
            {
                await writer.WriteAsync(message.ToString());
            }
            Console.ResetColor();
        }
        
        // Sync
        public static void WriteError<T>(this TextWriter writer, T message)
        {
            writer.WriteColor(message, ConsoleColor.DarkRed);
        }
        
        public static void WriteSuccess<T>(this TextWriter writer, T message)
        {
            writer.WriteColor(message, ConsoleColor.Green);
        }
        
        public static void WriteColor<T>(this TextWriter writer, T message, ConsoleColor color = ConsoleColor.White, bool newLine = true)
        {
            if (message == null)
            {
                writer.WriteLine();
                return;
            }
            
            Console.ForegroundColor = color;
            if (newLine)
            {
                writer.WriteLine(message);
            }
            else
            {
                writer.Write(message);
            }
            Console.ResetColor();
        }

        public static void WriteBuildSummary(this TextWriter writer, bool success, int errors, int warnings)
        {
            var color = success ? ConsoleColor.Green : ConsoleColor.DarkRed;
            writer.WriteColor($"Build {(success ? "Succeeded" : "Failed")}.\n" +
                              $"{warnings,5} Warnings(s)\n" +
                              $"{errors,5} Errors(s)\n", color);
        }

        public static void WriteKeyword(this TextWriter writer, SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);
            Debug.Assert(kind.IsKeyword() && text != null);
            writer.WriteKeyword(text);
        }

        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.WriteColor(text, ConsoleColor.Blue, newLine: false);
        }

        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.WriteColor(text, ConsoleColor.DarkYellow, newLine: false);
        }

        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.WriteColor(text, ConsoleColor.Cyan, newLine: false);
        }

        public static void WriteString(this TextWriter writer, string text)
        {
            writer.WriteColor(text, ConsoleColor.Magenta, newLine: false);
        }

        public static void WriteSpace(this TextWriter writer)
        {
            writer.WritePunctuation(" ");
        }

        public static void WriteComment(this TextWriter writer, string text)
        {
            writer.WriteColor($"// {text}", ConsoleColor.DarkGreen, newLine: false);
        }

        public static void WritePunctuation(this TextWriter writer, SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);
            Debug.Assert(text != null);

            writer.WritePunctuation(text);
        }

        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.WriteColor(text, ConsoleColor.DarkGray, newLine: false);
        }

        public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostic> diag)
        {
            var diagnostics = diag.ToList();
            
            foreach (var diagnostic in diagnostics.Where(d => d.Location.Text == null))
            {
                var messageColor = diagnostic.IsWarning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
                writer.WriteColor(diagnostic.Message, messageColor);
            }
 
            foreach (var diagnostic in diagnostics.Where(d => d.Location.Text != null)
                                                  .OrderBy(d => d.Location.FileName)
                                                  .ThenBy(d => d.Location.Span.Start)
                                                  .ThenBy(d => d.Location.Span.Length))
            {
                var text = diagnostic.Location.Text;
                var fileName = diagnostic.Location.FileName;
                
                var startLine = diagnostic.Location.StartLine + 1;
                var startCharacter = diagnostic.Location.StartCharacter + 1;
                var endLine = diagnostic.Location.EndLine + 1;
                var endCharacter = diagnostic.Location.EndCharacter + 1;
 
                var span = diagnostic.Location.Span;
                var lineIndex = text.GetLineIndex(span.Start);
                var line = text.Lines[lineIndex];
 
                writer.WriteLine();
 
                var messageColor = diagnostic.IsWarning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
                writer.WriteColor($"{fileName}: ({startLine}, {startCharacter} :: {endLine}, {endCharacter}): {diagnostic}", messageColor);
                
                var prefixSpan = TextSpan.FromBounds(line.Start, span.Start);
                var suffixSpan = TextSpan.FromBounds(span.End, line.End);
 
                var prefix = text.ToString(prefixSpan);
                var error = text.ToString(span);
                var suffix = text.ToString(suffixSpan);
 
                writer.Write($"    {prefix}");
               
                writer.WriteColor(error, ConsoleColor.DarkRed, newLine: false);
                
                writer.WriteLine(suffix);
            }
 
            writer.WriteLine();
        }
    }
}