using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Set : Command
{
        private readonly ILogger log;
        private readonly IConfiguration config;

        public Set(ILogger log, IConfiguration config) : base("set", "Set config values")
        {
            this.log = log;
            this.config = config;

            Option registry = new Option<string>(["--registry", "-r"], "Registry URL");
            registry.IsRequired = false;
            this.AddOption(registry);

            Option reset = new Option<bool>(["--default"], "Reset default config");
            reset.IsRequired = false;
            this.AddOption(reset);

            this.SetHandler((string registry, bool reset) => HandleSet(registry, reset), registry, reset);
        }

        private int HandleSet(string registry, bool reset)
        {
            var myConfig = new Config();
            config.Bind(myConfig);

            if(!string.IsNullOrEmpty(registry))
            {
                myConfig.Registry = registry;
                Config.Save(myConfig);
                log.Debug("Config: {@myConfig}", myConfig);
            }

            if(reset)
            {
                Config.Reset();
                log.Debug("Config: {@myConfig}", new Config());
            }

            return 0;
        }
}
