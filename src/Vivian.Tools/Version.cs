using SysVer = System.Version;

namespace Vivian.Tools
{
    public static class Version
    {
        public const int Major = 1;
        public const int Minor = 0;
        public const int Patch = 5;
        public const int Hotfix = 10;

        public static string ShortVersion { get; } = $"{Major}.{Minor}";
        public static string MidVersion { get; } = $"{Major}.{Minor}.{Patch}";
        public static string FullVersion { get; } = $"{Major}.{Minor}.{Patch}.{Hotfix}";
    }
}