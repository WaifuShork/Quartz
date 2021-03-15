using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document.Proposals;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Models.Proposals;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.VLanguageServer
{
    #pragma warning disable CS0618 // Type or member is obsolete
    internal sealed class SemanticTokensHandler : SemanticTokensHandlerBase
    {
        private readonly ILogger<SemanticTokensHandler> _logger;
        private readonly LspHost _lspHost;

        public SemanticTokensHandler(ILogger<SemanticTokensHandler> logger, LspHost lspHost) : base(new SemanticTokensRegistrationOptions
        {
            DocumentSelector = DocumentSelector.ForLanguage("ev2"),
            Legend = new SemanticTokensLegend(),
            Full = new SemanticTokensCapabilityRequestFull { Delta = true },
            Range = true
        })
        {
            _logger = logger;
            _lspHost = lspHost;
        }

        public override async Task<SemanticTokens?> Handle(SemanticTokensParams request, CancellationToken cancellationToken)
        {
            var result = await base.Handle(request, cancellationToken);
            return result;
        }

        public override async Task<SemanticTokens?> Handle(SemanticTokensRangeParams request, CancellationToken cancellationToken)
        {
            var result = await base.Handle(request, cancellationToken);
            return result;
        }

        public override async Task<SemanticTokensFullOrDelta?> Handle(
            SemanticTokensDeltaParams request,
            CancellationToken cancellationToken
        )
        {
            var result = await base.Handle(request, cancellationToken);
            return result;
        }

        private void TokenizeTokenTree(SemanticTokensBuilder builder, SyntaxNode token, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            switch (token.Kind)
            {
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                    _logger.LogInformation($"Token: {token.Kind}. Line: {token.Location.StartLine}. Char: {token.Span.Start}. Len: {token.Span.Length}");
                    builder.Push(token.Location.StartLine, token.Location.StartCharacter, token.Span.Length, SemanticTokenType.Comment, SemanticTokenModifier.Defaults);
                    break;

                case SyntaxKind.NumberToken:
                    _logger.LogInformation($"Token: {token.Kind}. Line: {token.Location.StartLine}. Char: {token.Span.Start}. Len: {token.Span.Length}");
                    builder.Push(token.Location.StartLine, token.Location.StartCharacter, token.Span.Length, SemanticTokenType.Number, SemanticTokenModifier.Defaults);
                    break;

                case SyntaxKind.StringToken:
                    _logger.LogInformation($"Token: {token.Kind}. Line: {token.Location.StartLine}. Char: {token.Span.Start}. Len: {token.Span.Length}");
                    builder.Push(token.Location.StartLine, token.Location.StartCharacter, token.Span.Length, SemanticTokenType.String, SemanticTokenModifier.Defaults);
                    break;

                case SyntaxKind.PlusToken:
                case SyntaxKind.PlusEqualsToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.MinusEqualsToken:
                case SyntaxKind.StarToken:
                case SyntaxKind.StarEqualsToken:
                case SyntaxKind.SlashToken:
                case SyntaxKind.SlashEqualsToken:
                case SyntaxKind.BangToken:
                case SyntaxKind.EqualsToken:
                case SyntaxKind.TildeToken:
                case SyntaxKind.HatToken:
                case SyntaxKind.HatEqualsToken:
                case SyntaxKind.AmpersandToken:
                case SyntaxKind.AmpersandAmpersandToken:
                case SyntaxKind.AmpersandEqualsToken:
                case SyntaxKind.PipeToken:
                case SyntaxKind.PipeEqualsToken:
                case SyntaxKind.PipePipeToken:
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.BangEqualsToken:
                case SyntaxKind.LessToken:
                case SyntaxKind.LessOrEqualsToken:
                case SyntaxKind.GreaterToken:
                case SyntaxKind.GreaterOrEqualsToken:
                    _logger.LogInformation($"Token: {token.Kind}. Line: {token.Location.StartLine}. Char: {token.Span.Start}. Len: {token.Span.Length}");
                    builder.Push(token.Location.StartLine, token.Location.StartCharacter, token.Span.Length, SemanticTokenType.Operator, SemanticTokenModifier.Defaults);
                    break;

                case SyntaxKind.IdentifierToken:
                    _logger.LogInformation($"Token: {token.Kind}. Line: {token.Location.StartLine}. Char: {token.Span.Start}. Len: {token.Span.Length}");
                    // TODO: Be more specific
                    builder.Push(token.Location.StartLine, token.Location.StartCharacter, token.Span.Length, SemanticTokenType.Member, SemanticTokenModifier.Defaults);
                    break;

                case SyntaxKind.BreakKeyword:
                case SyntaxKind.ContinueKeyword:
                case SyntaxKind.ElseKeyword:
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.ForKeyword:
                case SyntaxKind.FunctionKeyword:
                case SyntaxKind.IfKeyword:
                case SyntaxKind.ConstKeyword:
                case SyntaxKind.ReturnKeyword:
                case SyntaxKind.ToKeyword:
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.VarKeyword:
                case SyntaxKind.WhileKeyword:
                case SyntaxKind.DoKeyword:
                case SyntaxKind.ClassKeyword:
                    _logger.LogInformation($"Token: {token.Kind}. Line: {token.Location.StartLine}. Char: {token.Span.Start}. Len: {token.Span.Length}");
                    builder.Push(token.Location.StartLine, token.Location.StartCharacter, token.Span.Length, SemanticTokenType.Keyword, SemanticTokenModifier.Defaults);
                    break;

                case SyntaxKind.Parameter:
                    _logger.LogInformation($"Token: {token.Kind}. Line: {token.Location.StartLine}. Char: {token.Span.Start}. Len: {token.Span.Length}");
                    builder.Push(token.Location.StartLine, token.Location.StartCharacter, token.Span.Length, SemanticTokenType.Parameter, SemanticTokenModifier.Defaults);
                    break;
            }

            foreach (var child in token.GetChildren())
            {
                TokenizeTokenTree(builder, child, cancellationToken);
            }
        }

        #pragma warning disable 1998
        protected override async Task Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams identifier, CancellationToken cancellationToken)
        {
            var doc = _lspHost.Documents[identifier.TextDocument.Uri];

            if (doc.Compilation == null) { return; }

            foreach (var syntaxTree in doc.Compilation.SyntaxTrees)
            {
                TokenizeTokenTree(builder, syntaxTree.Root, cancellationToken);
            }
        }

        protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(ITextDocumentIdentifierParams @params, CancellationToken cancellationToken) 
            => Task.FromResult(new SemanticTokensDocument(GetRegistrationOptions().Legend));
    }
}