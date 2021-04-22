using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace Vivian.Installer
{
    internal static class Program
    {
        [SupportedOSPlatform("windows")]
        private static async Task<int> Main()
            => await Installer.Install();
    }
}