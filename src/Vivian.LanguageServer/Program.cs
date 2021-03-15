using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;

namespace Vivian.VLanguageServer
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            await MainAsync(args);
        }

        private static async Task MainAsync(string[] args)
        {
            var server = await LanguageServer.From(
                options => options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .ConfigureLogging(lb =>
                    {
                        lb.AddLanguageProtocolLogging()
                            .SetMinimumLevel(LogLevel.Debug);
                    })
                    .WithHandler<TextDocumentHandler>()
                    .WithHandler<SemanticTokensHandler>()
                    .WithServices(x => x.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace)))
                    .WithServices(s =>
                    {
                        s.AddSingleton(provider =>
                        {
                            var lsf = provider.GetService<ILanguageServerFacade>();
                            var loggerFactory = provider.GetService<ILoggerFactory>();
                            var logger = loggerFactory.CreateLogger<LspHost>();

                            return new LspHost(logger, lsf!);
                        });
                    })
                );
            await server.WaitForExit;
        }
    }
}