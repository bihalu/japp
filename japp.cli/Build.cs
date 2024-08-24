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

        Option packageDir = new Option<string>(["--input", "-i"], "Input package directory, if omitted use current directory")
        {
            IsRequired = false
        };
        AddOption(packageDir);

        this.SetHandler((string packageDir) => HandleBuild(packageDir), packageDir);
    }

    private int HandleBuild(string packageDir)
    {
        log.Debug("Build: packageDir={packageDir}", packageDir);
        
        return new Japp(log, config).Build(packageDir);
    }
}
