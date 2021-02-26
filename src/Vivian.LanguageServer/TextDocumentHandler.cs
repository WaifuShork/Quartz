using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace Vivian.VLanguageServer
{
    internal class TextDocumentHandler : ITextDocumentSyncHandler
        {
            private readonly ILogger<TextDocumentHandler> _logger;
            private readonly ILanguageServerConfiguration _configuration;
            private readonly ILanguageServerFacade _languageServer;
            private readonly LspHost _lspHost;
            private readonly DocumentSelector _documentSelector = new DocumentSelector(
                new DocumentFilter
                {
                    Language = "ev2"
                }
            );
    
            private SynchronizationCapability? _capability;
    
            public TextDocumentHandler(ILogger<TextDocumentHandler> logger,
                                       ILanguageServerConfiguration configuration,
                                       ILanguageServerFacade languageServer,
                                       LspHost lspHost)
            {
                _logger = logger;
                _configuration = configuration;
                _languageServer = languageServer;
                _lspHost = lspHost;
            }
    
            public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;
    
            public async Task<Unit> Handle(DidChangeTextDocumentParams notification, CancellationToken token)
            {
                _lspHost.DidChangeTextDocument(notification.TextDocument, notification.ContentChanges);
    
                // Clear existing diagnostics
                _lspHost.ClearDiagnotics(notification.TextDocument.Uri);
    
                await _lspHost.ValidateTextDocumentAsync(notification.TextDocument.Uri, token);
    
                //var diagnostics = await _lspHost.ValidateTextDocumentAsync(notification.TextDocument.Uri, token);
    
                //_lspHost.PublishDiagnostics(diagnostics, token);
    
                return Unit.Value;
            }
    
            TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions>.GetRegistrationOptions()
            {
                return new TextDocumentChangeRegistrationOptions
                {
                    DocumentSelector = _documentSelector,
                    SyncKind = Change
                };
            }
    
            public void SetCapability(SynchronizationCapability capability) => _capability = capability;
    
            public async Task<Unit> Handle(DidOpenTextDocumentParams notification, CancellationToken token)
            {
                _lspHost.DidOpenTextDocument(notification.TextDocument);
    
                // Clear existing diagnostics
                _lspHost.ClearDiagnotics(notification.TextDocument.Uri);
    
                await _lspHost.ValidateTextDocumentAsync(notification.TextDocument.Uri, token);
    
                //var diagnostics = await _lspHost.ValidateTextDocumentAsync(notification.TextDocument.Uri, token);
    
                //_lspHost.PublishDiagnostics(diagnostics, token);
    
                return Unit.Value;
            }
    
            TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
            {
                return new TextDocumentRegistrationOptions
                {
                    DocumentSelector = _documentSelector,
                };
            }
    
            public Task<Unit> Handle(DidCloseTextDocumentParams notification, CancellationToken token)
            {
                return Unit.Task;
            }
    
            public Task<Unit> Handle(DidSaveTextDocumentParams notification, CancellationToken token) => Unit.Task;
    
            TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions()
            {
                return new TextDocumentSaveRegistrationOptions
                {
                    DocumentSelector = _documentSelector,
                    IncludeText = true
                };
            }
    
            public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new TextDocumentAttributes(uri, "ev2");
        }
}