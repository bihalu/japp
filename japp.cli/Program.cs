using japp.lib;
using japp.lib.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.CommandLine;
using System.Reflection;

namespace japp.cli;

class Program
{
    public static async Task Main(string[] args)
    {
        // Root command
        var rootCommand = new RootCommand
        {
            Name = "japp",
            Description = @$"
   _                   
  (_)                  
   _  __ _ _ __  _ __  
  | |/ _` | '_ \| '_ \ 
  | | (_| | |_) | |_) |
  | |\__,_| .__/| .__/ 
 _/ |     | |   | |    Just another package program ;-)
|__/      |_|   |_|    Version {GetInformationalVersion()}"
        };

        // User config
        var userConfigPath = Helper.GetConfigPath();
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(userConfigPath, optional: false)
            .Build();

        var myConfig = Helper.BindConfig(config);

        // Logging
        Option logging = new Option<string>(["--logging", "-l"], "Set logging, default is console:information")
        {
            IsRequired = false
        };
        rootCommand.AddOption(logging);

        ILogger log = CreateLogger(args, myConfig);
        log.Debug("Version: {version}", GetInformationalVersion());
        log.Debug("Use config: {@myConfig}", myConfig);

        // Add sub commands
        rootCommand.AddCommand(new Config(log, config));
        rootCommand.AddCommand(new Create(log, config));
        rootCommand.AddCommand(new Build(log, config));
        rootCommand.AddCommand(new Pull(log, config));
        rootCommand.AddCommand(new Push(log, config));
        rootCommand.AddCommand(new Login(log, config));
        rootCommand.AddCommand(new Logout(log, config));

        await rootCommand.InvokeAsync(args);
    }

    private static ILogger CreateLogger(string[] args, ConfigModel config)
    {
        LoggerConfiguration loggerConfiguration = new();

        string logOutput = GetLogOutputAndLevel(args).logOutput;
        string logLevel = GetLogOutputAndLevel(args).logLevel;

        switch (logOutput)
        {
            case "console":
                loggerConfiguration.WriteTo.Console();
                break;

            case "file":
                string logFile = Path.Combine(config.TempDir, "japp.log");
                loggerConfiguration.WriteTo.File(logFile);
                break;

            default:
                loggerConfiguration.WriteTo.Console();
                break;
        }

        var loggingLevelSwitch = new LoggingLevelSwitch();
        loggerConfiguration.MinimumLevel.ControlledBy(loggingLevelSwitch);

        switch (logLevel)
        {
            case "verbose":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
                break;

            case "debug":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;
                break;

            case "information":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
                break;

            case "warning":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Warning;
                break;

            case "error":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Error;
                break;

            case "fatal":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
                break;

            case "off":
                loggingLevelSwitch.MinimumLevel = (LogEventLevel)1 + (int)LogEventLevel.Fatal;
                break;

            default:
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
                break;
        }

        return loggerConfiguration.CreateLogger();
    }

    private static (string logOutput, string logLevel) GetLogOutputAndLevel(string[] args)
    {
        string logOutput = "console";
        string logLevel = "information";

        try
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--logging") || args[i].StartsWith("-l"))
                {
                    if (args[i].Contains("="))
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
        }
        catch (Exception)
        {
            // Ignore unsupported logging string
        }

        return (logOutput, logLevel);
    }

    private static string? GetInformationalVersion()
    {
        return typeof(Japp).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    }
}
