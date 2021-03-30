using System;
using System.Collections.Generic;
using System.Threading;
using Vivian.CodeAnalysis;
using Vivian.Tools.Diagnostics;

namespace Vivian.Tools.Services
{
    public interface IHost : IDisposable
    {
        void RequestShutdown();

        void PublishDiagnostics(IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken = default);
    }
}