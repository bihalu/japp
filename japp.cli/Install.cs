using japp.lib;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Install : Command
{
    private readonly ILogger log;
    private readonly IConfiguration config;

    public Install(ILogger log, IConfiguration config) : base("install", "Install japp package")
    {
        this.log = log;
        this.config = config;

        Argument packageName = new Argument<string>("package", "Package name");
        AddArgument(packageName);

        Option values = new Option<string>(["--values", "-f"], "File with values in yaml format")
        {
            IsRequired = false
        };
        AddOption(values);

        Option packageDir = new Option<string>(["--input", "-i"], "Install package from input directory, if omitted package is extracted and installed from temp directory")
        {
            IsRequired = false
        };
        AddOption(packageDir);

        this.SetHandler((string packageName, string values, string packageDir) => HandleInstall(packageName, values, packageDir), packageName, values, packageDir);
    }

    private int HandleInstall(string packageName, string values, string packageDir)
    {
        log.Debug("Install: packageName={packageName}, values={values}, packageDir={packageDir}", packageName, values, packageDir);
        
        return new Japp(log, config).Install(packageName, values, packageDir);
    }
}
