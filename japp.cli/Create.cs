using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Create : Command
{
        private readonly ILogger log;
        private readonly IConfiguration config;

        public Create(ILogger log, IConfiguration config) : base("create", "Create package template")
        {
            this.log = log;
            this.config = config;

            Option template = new Option<string>(["--template", "-t"], "Name [batch, package or helm]");
            template.IsRequired = false;
            this.AddOption(template);

            this.SetHandler((string template) => HandleCreate(template), template);
        }

        private int HandleCreate(string template)
        {
            var myConfig = new Config();
            config.Bind(myConfig);

            switch(template)
            {
                case "batch":
                    break;

                case "package":
                    break;

                case "helm":
                    break;

                default:
                    log.Warning("Unkown template {template}, fallback to batch", template);
                    template = "batch";
                    break;
            }

            log.Debug("Create template for: {template}", template);

            // package.yml
            // logo.png
            // docs/readme.md
            
            return 0;
        }
}
