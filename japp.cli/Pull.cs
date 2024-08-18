using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Pull : Command
{
        private readonly ILogger log;
        private readonly IConfiguration config;

        public Pull(ILogger log, IConfiguration config) : base("pull", "pull package from registry")
        {
            this.log = log;
            this.config = config;

            Argument package = new Argument<string>("package", "package name");
            this.AddArgument(package);
            
            Option name = new Option<string>(["--output", "-o"], "local package name");
            name.IsRequired = false;
            this.AddOption(name);

            this.SetHandler((string package, string name) => HandlePull(package, name), package, name); 
        }

        private int HandlePull(string package, string name)
        {
            var myConfig = new Config();
            config.Bind(myConfig);

            string registry = myConfig.Registry;

            this.log.Information("podman pull {registry}/{package} -o{name}", registry, package, name);

            return 0;
        }
}
