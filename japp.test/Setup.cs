using japp.lib;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog;
using Serilog.Events;

namespace japp.test
{
    public class Setup
    {
        public ILogger log { get; private set; }
        public IConfiguration config { get; private set; }

        public Setup()
        {
            var loggingLevelSwitch = new LoggingLevelSwitch
            {
                MinimumLevel = LogEventLevel.Debug
            };

            log = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .CreateLogger();

            var userConfigPath = Helper.GetConfigPath();

            config = new ConfigurationBuilder()
                .AddJsonFile(userConfigPath, optional: false)
                .Build();
        }
    }
}
