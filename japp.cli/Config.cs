using japp.lib;
using japp.lib.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Config : Command
{
    private readonly ILogger log;
    private readonly IConfiguration config;

    public Config(ILogger log, IConfiguration config) : base("config", "Set config values")
    {
        this.log = log;
        this.config = config;

        Option registry = new Option<string>(["--registry", "-r"], "Set registry")
        {
            IsRequired = false
        };
        AddOption(registry);

        Option temp = new Option<string>(["--temp", "-t"], "Set temp folder")
        {
            IsRequired = false
        };
        AddOption(temp);

        Option cleanup = new Option<bool?>(["--cleanup", "-c"], "Set cleanup")
        {
            IsRequired = false
        };
        AddOption(cleanup);

        Option tlsVerify = new Option<bool?>(["--tls-verify"], "Set tls verify")
        {
            IsRequired = false
        };
        AddOption(tlsVerify);

        Option reset = new Option<bool>(["--reset"], "Reset default config")
        {
            IsRequired = false
        };
        AddOption(reset);

        this.SetHandler((string registry, string temp, bool? cleanup, bool? tlsVerify, bool reset) => 
            HandleConfig(registry, temp, cleanup, tlsVerify, reset), registry, temp, cleanup, tlsVerify, reset);
    }

    private int HandleConfig(string registry, string temp, bool? cleanup, bool? tlsVerify, bool reset)
    {
        var myConfig = Helper.BindConfig(config);

        if (!string.IsNullOrEmpty(registry))
        {
            myConfig.Registry = registry;
            Helper.SaveConfig(myConfig);
        }

        if (!string.IsNullOrEmpty(temp))
        {
            myConfig.TempFolder = temp;
            Helper.SaveConfig(myConfig);
        }

        if (null != cleanup)
        {
            myConfig.Cleanup = (bool)cleanup;
            Helper.SaveConfig(myConfig);
        }

        if (null != tlsVerify)
        {
            myConfig.TlsVerify = (bool)tlsVerify;
            Helper.SaveConfig(myConfig);
        }

        if (reset)
        {
            Helper.ResetConfig();
            myConfig = new ConfigModel();
        }

        var jsonConfig = JsonConvert.SerializeObject(myConfig, Formatting.Indented);
        log.Information("Config file: {path}\n {config}", Helper.GetConfigPath(), jsonConfig);

        return 0;
    }
}
