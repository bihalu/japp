using japp.lib;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Push : Command
{
    private readonly ILogger log;
    private readonly IConfiguration config;

    public Push(ILogger log, IConfiguration config) : base("push", "Push japp package to registry")
    {
        this.log = log;
        this.config = config;

        Argument packageName = new Argument<string>("package", "Package name");
        AddArgument(packageName);

        Option retag = new Option<bool>(["--retag", "-r"], "Retag and push container images")
        {
            IsRequired = false
        };
        AddOption(retag);

        this.SetHandler((string packageName, bool retag) => HandlePush(packageName, retag), packageName, retag);
    }

    private Task<int> HandlePush(string packageName, bool retag)
    {
        log.Debug("Push: packageName={packageName}, retag={retag}", packageName, retag);

        return Task.FromResult(new Japp(log, config).Push(packageName, retag));
    }
}
