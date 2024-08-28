using japp.lib.Models;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace japp.lib;

public class Japp : IJapp
{
    private readonly ILogger log;
    private readonly IConfiguration config;
    private readonly ConfigModel myConfig;

    public Japp(ILogger log, IConfiguration config)
    {
        this.log = log;
        this.config = config;
        myConfig = Helper.BindConfig(config);
    }

    public int Create(string packageDir)
    {
        return new Create().Execute(log, myConfig, packageDir);
    }

    public int Build(string packageDir)
    {
        return new Build().Execute(log, myConfig, packageDir);
    }

    public int Pull(string packageName, string outputDir)
    {
        return new Pull().Execute(log, myConfig, packageName, outputDir);
    }

    public int Push(string packageName, bool retag = false)
    {
        return new Push().Execute(log, myConfig, packageName, retag);
    }

    public int Login(string username, string password)
    {
        return new Login().Execute(log, myConfig, username, password);
    }

    public int Logout()
    {
        return new Logout().Execute(log, myConfig);
    }

    public int Install(string packageName, string? values, string packageDir)
    {
        return new Install().Execute(log, myConfig, packageName, values, packageDir);
    }
}
