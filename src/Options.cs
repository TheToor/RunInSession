using CommandLine;

namespace RunInSession
{
    public class Options
    {
        [Option('p', "path", Required = true, HelpText = "Path to the Application to be started")]
        public string Path { get; set; } 
        [Option('a', "arguments", Required = false, HelpText = "Arguments to be passed to the application")]
        public string Arguments { get; set; }
        [Option('w', "workingDirectory", Required = false, HelpText = "Working directory of the application. Defaults to the directory of the application")]
        public string WorkingDirectory { get; set; }
        [Option('l', "lockscreen", Required = false, HelpText = "Start on lockscreen")]
        public bool StartOnLockScreen { get; set; }
    }
}
