using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;

namespace RunInSession
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(RunOptions)
                    .WithNotParsed(HandleParseError);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void RunOptions(Options options)
        {
            if(!File.Exists(options.Path))
            {
                Console.WriteLine($"'{options.Path} not found'");
                return;
            }

            if (string.IsNullOrEmpty(options.WorkingDirectory))
            {
                options.WorkingDirectory = Path.GetDirectoryName(options.Path);
            }

            if (!Directory.Exists(options.WorkingDirectory))
            {
                Console.WriteLine($"'{options.WorkingDirectory} not found'");
            }


            Console.WriteLine($"Application: {options.Path}");
            Console.WriteLine($"Arguments: {options.Arguments}");
            Console.WriteLine($"WorkingDirectory: {options.WorkingDirectory}");
            Console.WriteLine($"StartInLockScreen: {options.StartOnLockScreen}");

            if(options.StartOnLockScreen)
            {
                ProcessUtil.StartProcessInLockScreen(options.Path, options.Arguments, options.WorkingDirectory);
            }
            else
            {
                ProcessUtil.StartProcessForAllSession(options.Path, options.Arguments, options.WorkingDirectory);
            }
        }
        static void HandleParseError(IEnumerable<Error> errors)
        {
            Console.WriteLine("Failed to parse input");
            foreach(var error in errors)
            {
                Console.WriteLine(error.ToString());
            }
        }
    }
}
