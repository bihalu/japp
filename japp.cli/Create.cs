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

        Option output = new Option<string>(["--output", "-o"], "Output folder")
        {
            IsRequired = false
        };
        AddOption(output);

        this.SetHandler((string output) => HandleCreate(output), output);
    }

    private int HandleCreate(string output)
    {
        log.Debug("Create: output={output}", output);

        return new Japp(log, config).Create(output);
    }
}
