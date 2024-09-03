using japp.lib;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Logout : Command
{
    private readonly ILogger log;
    private readonly IConfiguration config;

    public Logout(ILogger log, IConfiguration config) : base("logout", "Logout from registry")
    {
        this.log = log;
        this.config = config;

        this.SetHandler(() => HandleLogout());
    }

    private Task<int> HandleLogout()
    {
        log.Debug("Logout:");

        return Task.FromResult(new Japp(log, config).Logout());
    }
}
