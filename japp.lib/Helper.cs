using japp.lib.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using YamlDotNet.Core.Tokens;

namespace japp.lib;

public static class Helper
{
    public static (int returncode, string stdout, string stderr) RunCommand(ILogger log, string command, bool useShell = false, string? workingDir = null)
    {
        int returncode = 0;
        string stdout = string.Empty;
        string stderr = string.Empty;

        if (string.IsNullOrEmpty(workingDir))
        {
            workingDir = Environment.CurrentDirectory;
        }

        // Wrap command in shell
        if (useShell)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                command = $"cmd /c {command}";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (command.Contains('"') && command.Contains('\''))
                {
                    returncode = -1;
                    stderr = "Command contains quotes and double quotes, can not mix within shell!";
                    log.Error(stderr);
                    return (returncode, stdout, stderr);
                }

                if (command.Contains('"'))
                {
                    command = $"sh -c '{command}'";
                }
                else
                {
                    command = $"sh -c \"{command}\"";
                }
            }
        }

        // Extract command line args
        string args = string.Empty;
        if (command.Contains(' '))
        {
            int i = command.IndexOf(' ');
            args = command[(i + 1)..];
            command = command[..i];
        }

        // Reduct password in args
        string reductedArgs = string.Empty;
        bool reduct = false;
        foreach (var arg in args.Split(' '))
        {
            if (reduct)
            {
                reductedArgs += new string('*', arg.Length) + ' ';
            }
            else
            {
                reductedArgs += arg + ' ';
            }

            reduct = arg.Contains("password", StringComparison.CurrentCultureIgnoreCase);
        }

        log.Debug("Command: {command} {args}", command, reductedArgs);

        // Set process start info
        ProcessStartInfo processStartInfo = new()
        {
            UseShellExecute = false, // No GUI
            CreateNoWindow = true, // No Window
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDir,
            FileName = command,
            Arguments = args,
            EnvironmentVariables = { { "FOO", "BAR" }, { "BAR", "FOO" } },
            Environment = { { "FOOBAR", "BARFOO" }, { "BARFOO", "FOOBAR" } }
        };

        log.Verbose("Process start info: {@processStartInfo}", processStartInfo);

        // Start process
        using (var process = new Process { StartInfo = processStartInfo })
        {
            var stopwatch = new Stopwatch();
            returncode = 0;

            StringBuilder outputData = new();
            StringBuilder errorData = new();

            process.OutputDataReceived += (sender, data) =>
            {
                if (null != data.Data)
                {
                    outputData.AppendLine(data.Data);
                    log.Debug("Stdout: {stdout}", data.Data);
                }
            };

            process.ErrorDataReceived += (sender, data) =>
            {
                if (null != data.Data)
                {
                    errorData.AppendLine(data.Data);
                    log.Debug("Stderr: {stderr}", data.Data);
                }
            };

            try
            {
                stopwatch.Start();

                using (LogContext.PushProperty("Command", command))
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    returncode = process.ExitCode;
                }

                stdout = outputData.ToString().TrimEnd('\r').TrimEnd('\n');
                stderr = errorData.ToString().TrimEnd('\r').TrimEnd('\n');
            }
            catch (Exception exception)
            {
                returncode = 1;
                stderr = exception.Message;
                log.Error(exception.StackTrace!);
            }

            stopwatch.Stop();
            
            if (returncode == 0)
            {
                log.Debug("Command: {command} {args} (Returncode: {returncode}, Duration: {elapsed})", command, reductedArgs, returncode, stopwatch.Elapsed);
            }
            else
            {
                foreach (var error in stderr.Split('\n'))
                {
                    log.Error("{error}", error);
                }

                log.Error("Command: {command} {args} (Returncode: {returncode}, Duration: {elapsed})", command, reductedArgs, returncode, stopwatch.Elapsed);
            }

            var myEnv = process.StartInfo.Environment;

            log.Debug("myEnv count: {count}", myEnv.Count);

            foreach (var key in myEnv.Keys)
            {
                log.Debug($"{key}={myEnv[key]}");
            }

            var myEnvVar = process.StartInfo.EnvironmentVariables;

            log.Debug("myEnvVar count: {count}", myEnvVar.Count);

            foreach (var key in myEnvVar.Keys)
            {
                log.Debug("{key}", key);
            }

            foreach (var value in myEnvVar.Values)
            {
                log.Debug("{value}", value);
            }

        };

        return (returncode, stdout, stderr);
    }

    public static string GetConfigPath()
    {
        var userProfile = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        var userDir = Path.Combine(userProfile.FullName, ".japp");
        var userConfigPath = Path.Combine(userDir, "config.json");

        if (!Directory.Exists(userDir))
        {
            Directory.CreateDirectory(userDir);
        };

        if (!File.Exists(userConfigPath))
        {
            var newConfig = JsonConvert.SerializeObject(new ConfigModel(), Formatting.Indented);
            File.WriteAllText(userConfigPath, newConfig);
        }

        return userConfigPath;
    }

    public static bool SaveConfig(ConfigModel config)
    {
        var newConfig = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(GetConfigPath(), newConfig);

        return true;
    }

    public static bool ResetConfig()
    {
        return SaveConfig(new ConfigModel());
    }

    public static ConfigModel BindConfig(IConfiguration config)
    {
        var myConfig = new ConfigModel();
        config.Bind(myConfig);
        return myConfig;
    }
}
