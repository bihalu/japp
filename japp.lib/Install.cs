using ICSharpCode.SharpZipLib.Tar;
using japp.lib.Models;
using japp.lib.Runner;
using Serilog;
using System.Text;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace japp.lib
{
    internal class Install
    {
        public int Execute(ILogger log, ConfigModel myConfig, string packageName, string? values, string packageDir)
        {
            string registry = myConfig.Registry;
            string tempDir = Path.Combine(myConfig.TempDir, Guid.NewGuid().ToString("N"));

            if (string.IsNullOrEmpty(packageDir))
            {
                // Validate container image
                var validateResult = new Validate().Image(log, myConfig, packageName);
                if (validateResult.returncode != 0)
                {
                    return validateResult.returncode;
                }

                // Save container image
                var saveResult = Helper.RunCommand(log, $"podman save --format=docker-dir {registry}/{packageName} --output {tempDir}");

                if (saveResult.returncode != 0)
                {
                    return saveResult.returncode;
                }

                // Extract japp package to temp package dir
                packageDir = Path.Combine(myConfig.TempDir, Guid.NewGuid().ToString("N"));

                string sourceLayer = Path.Combine(tempDir, validateResult.layer);

                using Stream inputStream = File.OpenRead(sourceLayer);
                using TarArchive tarArchive = TarArchive.CreateInputTarArchive(inputStream, Encoding.Default);
                tarArchive.ExtractContents(packageDir);
                tarArchive.Close();
                inputStream.Close();
            }
            else
            {
                // Validate package source
                var validateResult = new Validate().Source(log, myConfig, packageDir);
                if (validateResult.returncode != 0)
                {
                    return validateResult.returncode;
                }
            }

            // Package
            string packagePath = Path.Combine(packageDir, "package.yml");
            var yaml = File.ReadAllText(packagePath);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var package = deserializer.Deserialize<PackageModel>(yaml);

            // Cleanup .japp_env
            var environemtVariablesPath = Path.Combine(packageDir, ".japp_env");
            if (File.Exists(environemtVariablesPath))
            {
                File.Delete(environemtVariablesPath);
            }

            // Task runner
            // TODO values for task runner
            // TODO useShell
            var runner = new TaskRunner(log, myConfig);
            int returncode = runner.Run(package.Install, packageDir, useShell: true);

            return returncode;
        }
    }
}
