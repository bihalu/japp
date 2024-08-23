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
        log.Debug("Pull: package={package}, output={output}", package, output);

        return new Japp(log, config).Pull(package, output);
    }
}
