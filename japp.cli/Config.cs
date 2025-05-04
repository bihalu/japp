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

        Option tempDir = new Option<string>(["--temp", "-t"], "Set temp directory")
        {
            IsRequired = false
        };
        AddOption(tempDir);

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

        this.SetHandler((string registry, string tempDir, bool? cleanup, bool? tlsVerify, bool reset) => 
            HandleConfig(registry, tempDir, cleanup, tlsVerify, reset), registry, tempDir, cleanup, tlsVerify, reset);
    }

    private Task<int> HandleConfig(string registry, string tempDir, bool? cleanup, bool? tlsVerify, bool reset)
    {
        log.Debug("Config: registry={registry}, tempDir={tempDir}, cleanup={cleanup}, tlsVerify={tlsVerify}, reset={reset}", registry, tempDir, cleanup, tlsVerify, reset);

        var myConfig = Helper.BindConfig(config);

        if (!string.IsNullOrEmpty(registry))
        {
            myConfig.Registry = registry;
            Helper.SaveConfig(myConfig);
        }

        if (!string.IsNullOrEmpty(tempDir))
        {
            myConfig.TempDir = tempDir;
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
        log.Information("Config file: {path}\n{config}", Helper.GetConfigPath(), jsonConfig);

        return System.Threading.Tasks.Task.FromResult(0);
    }
}
