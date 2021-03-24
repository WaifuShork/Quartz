using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Vivian.Build.Tasks
{
    public class VivianCompile : ToolTask
    {
        private ITaskItem[]? _sources;
        private ITaskItem? _outputAssembly;
        private ITaskItem[]? _references;

        protected override string ToolName => ToolExe;

        public ITaskItem[]? Sources
        {
            set => _sources = value;
        }

        [Output]
        public ITaskItem? OutputAssembly
        {
            set => _outputAssembly = value;
        }

        public ITaskItem[]? References
        {
            set => _references = value;
        }

        [Output]
        public ITaskItem[]? CommandLineArgs { get; set; }

        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(ToolPath, ToolExe);
        }

        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();
            
            builder.AppendFileNamesIfNotNull(_sources, " ");
            builder.AppendSwitchIfNotNull("/o ", _outputAssembly);

            if (_references != null)
            {
                foreach (var refItem in _references)
                {
                    builder.AppendSwitchIfNotNull("/r ", refItem);
                }
            }

            return builder.ToString();
        }
    }
}