using japp.lib;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Pull : Command
{
    private readonly ILogger log;
    private readonly IConfiguration config;

    public Pull(ILogger log, IConfiguration config) : base("pull", "Pull package from registry")
    {
        this.log = log;
        this.config = config;

        Argument package = new Argument<string>("package", "Package name");
        AddArgument(package);

        Option output = new Option<string>(["--output", "-o"], "Output folder")
        {
            IsRequired = false
        };
        AddOption(output);

        this.SetHandler((string package, string output) => HandlePull(package, output), package, output);
    }

    private int HandlePull(string package, string output)
    {
        var myConfig = Helper.BindConfig(config);

        string registry = myConfig.Registry;
        string options = myConfig.TlsVerify ? "" : "--tls-verify=false";
        string temp = Path.Combine(myConfig.TempFolder, Guid.NewGuid().ToString("N"));

        // Pull container image
        var pullResult = Helper.RunCommand(log, $"podman pull {options} {registry}/{package}");
        if (pullResult.returncode != 0) return pullResult.returncode;

        // Inspect container image
        var inspectResult = Helper.RunCommand(this.log, $"podman inspect {registry}/{package}");
        if (inspectResult.returncode != 0) return inspectResult.returncode;

        string json = inspectResult.stdout.TrimStart('[').TrimEnd('\n').TrimEnd('\r').TrimEnd(']');
        var containerImage = JsonConvert.DeserializeObject<dynamic>(json)!;

        // Gather metadata
        string id = containerImage.Id;
        var annotations = containerImage.Annotations;
        string graphDriverName = containerImage.GraphDriver.Name;
        string upperDir = containerImage.GraphDriver.Data.UpperDir;

        // Save container image
        var saveResult = Helper.RunCommand(this.log, $"podman save --format=docker-dir {registry}/{package} --output {temp}");

        return 0;
    }
}
