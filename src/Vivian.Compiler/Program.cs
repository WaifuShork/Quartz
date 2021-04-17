using System.Threading.Tasks;
using Vivian.Tools;

namespace VivianCompiler
{
    internal static class Program
    {
        private static void Main(string[] args)
            => new VivianTools().RunVivianTools(args);
    }
}