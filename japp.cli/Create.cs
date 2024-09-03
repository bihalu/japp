using japp.lib;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Create : Command
{
    private readonly ILogger log;
    private readonly IConfiguration config;

    public Create(ILogger log, IConfiguration config) : base("create", "Create japp package template")
    {
        this.log = log;
        this.config = config;

        Option outputDir = new Option<string>(["--output", "-o"], "Output directory, if omitted use current directory")
        {
            IsRequired = false
        };
        AddOption(outputDir);

        this.SetHandler((string outputDir) => HandleCreate(outputDir), outputDir);
    }

    private Task<int> HandleCreate(string outputDir)
    {
        log.Debug("Create: outputDir={outputDir}", outputDir);

        return Task.FromResult(new Japp(log, config).Create(outputDir));
    }
}
