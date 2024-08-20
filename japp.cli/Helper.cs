using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;
using Serilog.Context;

namespace japp.cli;

public static class Helper
{
    public static (int returncode, string stdout, string stderr) RunCommand(ILogger log, string command, bool useShell = false, string? dir = null)
    {
        int returncode = 0;
        string stdout = string.Empty;
        string stderr = string.Empty;

        if(string.IsNullOrEmpty(dir)){
            dir = Environment.CurrentDirectory;
        }

        // Wrap command in shell
        if(useShell)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                command = $"cmd /c {command}";
            }

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if(command.Contains('"') && command.Contains('\''))
                {
                    returncode = -1;
                    stderr = "Command contains quotes and double quotes! can not mix within shell!";
                    log.Error(stderr);
                    return (returncode, stdout, stderr);
                }

                if(command.Contains('"'))
                {
                    command = $"bash -c '{command}'";
                }
                else
                {
                    command = $"bash -c \"{command}\"";
                }
            }
        }

        // Extract command line args
        string args = string.Empty;
        if(command.Contains(' '))
        {
            int i = command.IndexOf(' ');
            args = command.Substring(i+1);
            command = command.Substring(0, i);
        }

        log.Debug("Command: {command} {args}", command, args);

        // Set process start info
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = false, // No GUI
            CreateNoWindow = true, // No Window
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = dir,
            FileName = command,
            Arguments = args
        };

        log.Verbose("Process start info: {@processStartInfo}", processStartInfo);

        // Start process
        using(var process = new Process { StartInfo = processStartInfo })
        {
            var stopwatch = new Stopwatch();
            returncode = 0;

            StringBuilder outputData = new StringBuilder();
            StringBuilder errorData = new StringBuilder();

            process.OutputDataReceived += (sender, data) => {
                if(null != data.Data)
                {
                    outputData.AppendLine(data.Data);
                    log.Debug("Stdout: {stdout}", data.Data);
                }
            };

            process.ErrorDataReceived += (sender, data) =>
            {
                if(null != data.Data)
                {
                    errorData.AppendLine(data.Data);
                    log.Debug("Stderr: {stderr}", data.Data);
                }
            };

            try
            {
                log.Verbose("Start command {command}...", command);
                stopwatch.Start();

                using(LogContext.PushProperty("Command", command))
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
            catch(Exception exception)
            {
                log.Error(exception.StackTrace!);
            }

            stopwatch.Stop();
            if(returncode == 0)
            {
                log.Information("Command: {command} {args} (Returncode: {returncode}, Duration: {elapsed})", command, args, returncode, stopwatch.Elapsed);
            }
            else
            {
                log.Error("Command: {command} {args} (Returncode: {returncode}, Duration: {elapsed})", command, args, returncode, stopwatch.Elapsed);
            }
        };

        return (returncode, stdout, stderr);
    }
}