using japp.lib;
using Microsoft.Extensions.Configuration;
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

        Argument packageName = new Argument<string>("package", "Package name");
        AddArgument(packageName);

        Option outputDir = new Option<string>(["--output", "-o"], "Output directory")
        {
            IsRequired = false
        };
        AddOption(outputDir);

        this.SetHandler((string packageName, string outputDir) => HandlePull(packageName, outputDir), packageName, outputDir);
    }

    private int HandlePull(string packageName, string outputDir)
    {
        log.Debug("Pull: packageName={packageName}, outputDir={output}", packageName, outputDir);

        return new Japp(log, config).Pull(packageName, outputDir);
    }
}
