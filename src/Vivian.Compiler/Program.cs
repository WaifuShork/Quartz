using Vivian.Tools;

namespace VivianCompiler
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var vivianTools = new VivianTools();
            vivianTools.RunVivianTools(args);
            return 0;
        }
    }
}