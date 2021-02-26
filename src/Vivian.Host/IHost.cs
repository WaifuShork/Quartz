using System;
using System.Collections.Generic;
using System.Threading;
using Vivian.CodeAnalysis;

namespace Vivian.Host
{
    public interface IHost : IDisposable
    {
        void RequestShutdown();

        void PublishDiagnostics(IEnumerable<IDiagnostic> diagnostics, CancellationToken cancellationToken = default);
    }
}