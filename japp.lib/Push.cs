using japp.lib.Models;
using Serilog;
using System.Text;
using ICSharpCode.SharpZipLib.Tar;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace japp.lib
{
    internal class Push
    {
        public int Execute(ILogger log, ConfigModel myConfig, string packageName, bool retagAndPush = false)
        {
            string registry = myConfig.Registry;
            string options = myConfig.TlsVerify ? "" : "--tls-verify=false "; // Workaround for self signed registry cert
            string tempDir = Path.Combine(myConfig.TempDir, Guid.NewGuid().ToString("N"));

            // Validate container image
            var validateResult = new Validate().Image(log, myConfig, packageName);
            if (validateResult.returncode != 0)
            {
                return validateResult.returncode;
            }

            // Push container image
            var pushResult = Helper.RunCommand(log, $"podman push {options}{registry}/{packageName}");

            if (pushResult.returncode != 0)
            {
                return pushResult.returncode;
            }

            log.Information("Push japp package {registry}/{packageName}", registry, packageName);

            // Retag and push container images
            if (retagAndPush)
            {
                // Save container image
                var saveResult = Helper.RunCommand(log, $"podman save --format=docker-dir {registry}/{packageName} --output {tempDir}");

                if (saveResult.returncode != 0)
                {
                    return saveResult.returncode;
                }

                // Extract japp package inside temp dir
                string sourceLayer = Path.Combine(tempDir, validateResult.layer);

                using Stream inputStream = File.OpenRead(sourceLayer);
                using TarArchive tarArchive = TarArchive.CreateInputTarArchive(inputStream, Encoding.Default);
                tarArchive.ExtractContents(tempDir);
                tarArchive.Close();
                inputStream.Close();

                string packageYml = Path.Combine(tempDir, "package.yml");
                var yml = File.ReadAllText(packageYml);
                log.Debug("{packageYaml}:\n{yaml}", packageYml, yml);

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var package = deserializer.Deserialize<PackageModel>(File.ReadAllText(packageYml));

                if (package.Containers != null && package.Containers.Any())
                {
                    // Retag and push all container images in japp package
                    foreach (var container in package!.Containers)
                    {
                        string sourceImage = $"{container.Registry}/{container.Image}:{container.Tag}";
                        string destinationImage = $"{registry}/{container.Image}:{container.Tag}";

                        // Tag container image
                        var retagResult = Helper.RunCommand(log, $"podman tag {sourceImage} {destinationImage}");

                        if (retagResult.returncode != 0)
                        {
                            return retagResult.returncode;
                        }

                        // Push container image
                        var pushContainerResult = Helper.RunCommand(log, $"podman push {options}{destinationImage}");

                        if (pushContainerResult.returncode != 0)
                        {
                            return pushContainerResult.returncode;
                        }

                        log.Information("Push container image {destinationImage}", destinationImage);
                    }
                }
            }

            return 0;
        }
    }
}
