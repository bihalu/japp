using japp.lib;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Push : Command
{
    private readonly ILogger log;
    private readonly IConfiguration config;

    public Push(ILogger log, IConfiguration config) : base("push", "Push package and images to registry")
    {
        this.log = log;
        this.config = config;

        Argument packageName = new Argument<string>("package", "Package name");
        AddArgument(packageName);

        this.SetHandler((string packageName) => HandlePush(packageName), packageName);
    }

    private int HandlePush(string packageName)
    {
        log.Debug("Push: packageName={packageName}", packageName);

        return new Japp(log, config).Push(packageName);
    }
}
