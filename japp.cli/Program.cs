using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

class Program
{
    public static async Task Main(string[] args)
    {
        // logger
        ILogger log = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        // user config
        var userConfigPath = Config.GetPath();
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(userConfigPath, optional: true, reloadOnChange: true)
            .Build();

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
|__/      |_|   |_|    ";

        // add sub commands
        rootCommand.AddCommand(new Pull(log, config));

        await rootCommand.InvokeAsync(args);
    }
}
