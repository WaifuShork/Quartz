using System;
using System.Collections.Generic;
using System.Threading;
using Vivian.CodeAnalysis;
using Vivian.Diagnostics;

namespace Vivian.Host
{
    public interface IHost : IDisposable
    {
        void RequestShutdown();

        void PublishDiagnostics(IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken = default);
    }
}