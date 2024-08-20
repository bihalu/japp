using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
            
            Option output = new Option<string>(["--output", "-o"], "output folder");
            output.IsRequired = false;
            this.AddOption(output);

            this.SetHandler((string package, string output) => HandlePull(package, output), package, output); 
        }

        private int HandlePull(string package, string output)
        {
            var myConfig = new Config();
            config.Bind(myConfig);

            string registry = myConfig.Registry;

            // pull container image
            var pullResult = Helper.RunCommand(this.log, $"podman pull {registry}/{package}");
            if(pullResult.returncode != 0) return pullResult.returncode;

            // inspect container image
            var inspectResult = Helper.RunCommand(this.log, $"podman inspect {registry}/{package}");
            if(inspectResult.returncode != 0) return inspectResult.returncode;

            string json = inspectResult.stdout.TrimStart('[').TrimEnd('\n').TrimEnd('\r').TrimEnd(']');
            var containerImage = JsonConvert.DeserializeObject<dynamic>(json)!;

            // gather metadata
            string id = containerImage.Id;
            var annotations = containerImage.Annotations;
            string graphDriverName = containerImage.GraphDriver.Name;
            string upperDir = containerImage.GraphDriver.Data.UpperDir;
            
            // list container image content
            var listResult = Helper.RunCommand(this.log, $"ls -la {upperDir}");

            // create output folder
            if(!string.IsNullOrEmpty(output))
            {
                if(!Directory.Exists(output)) Directory.CreateDirectory(output);

                if(Directory.EnumerateFileSystemEntries(output).Any())
                {
                    this.log.Warning("Output folder {output} is not empty", output);
                    return 1;
                }

                // copy content to output folder

            }

            return 0;
        }
}
