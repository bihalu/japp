using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

class Program
{
    public static async Task Main(string[] args)
    {
        // user config
        var userConfigPath = Config.GetPath();
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(userConfigPath, optional: true, reloadOnChange: true)
            .Build();

        // root command
        var rootCommand = new RootCommand();
        rootCommand.Name = "japp";
        rootCommand.Description = @"
   _                   
  (_)                  
   _  __ _ _ __  _ __  
  | |/ _` | '_ \| '_ \ 
  | | (_| | |_) | |_) |
  | |\__,_| .__/| .__/ 
 _/ |     | |   | |    
|__/      |_|   |_|    just another package program ;-)";

        // logging
        Option logging = new Option<string>(["--logging", "-l"], "set logging, default is console:information");
        logging.IsRequired = false;
        rootCommand.AddOption(logging);

        ILogger log = CreateLogger(args);

        // add sub commands
        rootCommand.AddCommand(new Pull(log, config));

        await rootCommand.InvokeAsync(args);
    }

    private static ILogger CreateLogger(string[] args)
    {
        LoggerConfiguration loggerConfiguration= new();

        string logOutput = GetLogOutputAndLevel(args).Item1;
        string logLevel = GetLogOutputAndLevel(args).Item2;

        switch (logOutput)
        {
            case "console":
                loggerConfiguration.WriteTo.Console();
                break;

            case "file":
                loggerConfiguration.WriteTo.File("/tmp/japp.log");
                break;

            default:
                break;
        }

        switch (logLevel)
        {
            case "verbose":
                loggerConfiguration.MinimumLevel.Verbose();
                break;

            case "debug":
                loggerConfiguration.MinimumLevel.Debug();
                break;

            case "error":
                loggerConfiguration.MinimumLevel.Error();
                break;

            case "warning":
                loggerConfiguration.MinimumLevel.Warning();
                break;

            case "information":
                loggerConfiguration.MinimumLevel.Information();
                break;

            default:
                break;
        }

        return loggerConfiguration.CreateLogger();
    }

    private static (string, string) GetLogOutputAndLevel(string [] args)
    {
        string logOutput = "console";
        string logLevel = "information";

        for(int i = 0; i < args.Length; i++)
        {
            if(args[i].StartsWith("--logging") || args[i].StartsWith("-l"))
            {
                if(args[i].Contains("="))
                {
                    string logging = args[i].Split('=')[1].ToLower();
                    logOutput = logging.Split(':')[0];
                    logLevel = logging.Split(':')[1];
                }
                else
                {
                    string logging = args[i + 1].ToLower();
                    logOutput = logging.Split(':')[0];
                    logLevel = logging.Split(':')[1];
                }
            }
        }

        return (logOutput, logLevel);
    }
}
