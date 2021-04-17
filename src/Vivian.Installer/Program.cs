using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Vivian.Installer
{
    internal static class Program
    {
        [SupportedOSPlatform("windows")]
        private static async Task Main()
            => await new Installer().Install();
    }
}