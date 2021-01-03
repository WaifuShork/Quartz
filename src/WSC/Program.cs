using System.Net.NetworkInformation;

namespace wsc
{
    internal static class Program
    {
        internal static void Main()
        {
            var repl = new VivianRepl();
            repl.Run();
        }
    }
}