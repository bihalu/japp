using japp.lib;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Build : Command
{
    private readonly ILogger log;
    private readonly IConfiguration config;

    public Build(ILogger log, IConfiguration config) : base("build", "Build japp package")
    {
        this.log = log;
        this.config = config;

        Option input = new Option<string>(["--input", "-i"], "Input folder")
        {
            IsRequired = false
        };
        AddOption(input);

        this.SetHandler((string input) => HandleBuild(input), input);
    }

    private int HandleBuild(string input)
    {
        return new Japp(log, config).Build(input);
    }
}
