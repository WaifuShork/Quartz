using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vivian.Host;
using System.Linq;
using Vivian.CompilerService;
using System.Threading;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Microsoft.Extensions.Logging;
using Uri = OmniSharp.Extensions.LanguageServer.Protocol.DocumentUri;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using Vivian.CodeAnalysis.Syntax;
using Vivian.CodeAnalysis;

namespace Vivian.VLanguageServer
{
    internal class TextDocumentInfo
    {
        public TextDocumentInfo(TextDocumentItem document, Compilation? compilation)
        {
            Document = document;
            Compilation = compilation;
        }
        
        public TextDocumentItem Document { get; }
        public Compilation? Compilation { get; set; }
    }

    internal sealed class LspHost : IHost
    {
        private readonly Uri? _workerSpaceRoot;
        private readonly int _maxNumberOfProblems = 100;
        private readonly Server _server;
        private readonly ILogger<LspHost> _logger;
        private readonly ILanguageServerFacade _languageServer; 
        
        internal IDictionary<Uri, TextDocumentInfo> Documents { get; }
        
        public LspHost(ILogger<LspHost> logger, ILanguageServerFacade languageServer)
        {
            Documents = new Dictionary<Uri, TextDocumentInfo>();
            _server = new Server(this);
            _logger = logger;
            _languageServer = languageServer;
        }

        internal void DidOpenTextDocument(TextDocumentItem document)
        {
            _logger.LogInformation($"Document opened {document.Uri.GetFileSystemPath()} ({document.Version})");
            Documents.Add(document.Uri, new TextDocumentInfo(document, null));
        }

        internal void DidChangeTextDocument(VersionedTextDocumentIdentifier document, Container<TextDocumentContentChangeEvent> changes)
        {
            _logger.LogInformation($"Document changed {document.Uri.GetFileSystemPath()} ({document.Version})");

            Uri docUri = document.Uri;

            if (!Documents.ContainsKey(docUri))
            {
                return;
            }

            var doc = Documents[docUri];

            // We only handle a full document update right now
            doc.Document.Version = document.Version;
            doc.Document.Text = changes.First().Text;
        }

        internal void DidCloseTextDocument(TextDocumentItem document)
        {
            _logger.LogInformation($"Document closed {document.Uri.GetFileSystemPath()} ({document.Version})");

            Uri docUri = document.Uri;
            if (Documents.ContainsKey(docUri))
            {
                Documents.Remove(docUri);
            }
        }

        internal async Task<SyntaxTree> ParseTextDocumentAsync(Uri docUri, CancellationToken cancellationToken)
        {
            var source = new List<string>(1)
            {
                docUri.GetFileSystemPath()
            };

            var syntaxTrees = await _server.Parse(source, cancellationToken);

            return syntaxTrees.FirstOrDefault();
        }

        internal async Task<Compilation?> ValidateTextDocumentAsync(Uri docUri, CancellationToken cancellationToken)
        {
            if (!Documents.ContainsKey(docUri))
            {
                _logger.LogError("File is not open for validation.");
                return null;
            }

            var doc = Documents[docUri];

            doc.Compilation = await _server.Validate(doc.Document.Text, doc.Document.Uri.GetFileSystemPath(), cancellationToken);

            return doc.Compilation;
        }

        public void RequestShutdown()
        {
            _logger.LogInformation("Compiler requested shutdown.");
        }

        public void ClearDiagnotics(Uri documentUri)
        {
            _languageServer.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams()
            {
                Uri = documentUri,
                Diagnostics = Array.Empty<OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic>()
            });
        }

        public void PublishDiagnostics(IEnumerable<IDiagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var groupedDiagnostics = diagnostics.GroupBy(k => k.DiagnosticLocation.Uri);

            foreach (var kv in groupedDiagnostics)
            {
                _languageServer.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
                {
                    Uri = kv.Key,
                    Diagnostics = kv.Select(d => new OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic()
                    {
                        Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                            new OmniSharp.Extensions.LanguageServer.Protocol.Models.Position(d.DiagnosticLocation.Range.Start.Line, d.DiagnosticLocation.Range.Start.Character),
                            new OmniSharp.Extensions.LanguageServer.Protocol.Models.Position(d.DiagnosticLocation.Range.End.Line, d.DiagnosticLocation.Range.End.Character)
                        ),
                        Severity = d.IsError ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning,
                        Source = "EV2",
                        Message = d.Message
                    }).ToArray()
                });

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public void Dispose()
        {
        }
    }
}