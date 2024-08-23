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

        Argument package = new Argument<string>("package", "Package name");
        AddArgument(package);

        this.SetHandler((string package) => HandlePush(package), package);
    }

    private int HandlePush(string package)
    {
        log.Debug("Push: package={package}", package);

        return new Japp(log, config).Push(package);
    }
}
