using japp.lib.Models;
using Serilog;
using System.Text;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace japp.lib
{
    internal class Build
    {
        public int Execute(ILogger log, ConfigModel myConfig, string packageDir)
        {
            // Check package dir
            if (string.IsNullOrWhiteSpace(packageDir))
            {
                packageDir = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(packageDir))
            {
                log.Error("Missing package dir {packageDir}, can't build japp package", packageDir);

                return 1;
            }

            // Read package.yml
            PackageModel package;
            string packageYml = Path.Combine(packageDir, "package.yml");

            if (!File.Exists(packageYml))
            {
                log.Error("Missing file {package}, can't build japp package", packageYml);

                return 2;
            }
            else
            {
                var yml = File.ReadAllText(packageYml);
                log.Debug("{packageYaml}:\n{yaml}", packageYml, yml);

                try
                {
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    package = deserializer.Deserialize<PackageModel>(File.ReadAllText(packageYml));
                }
                catch (Exception exception)
                {
                    log.Error("Invalid japp package {package} => {error}", packageYml, exception.Message);

                    return 3;
                }
            }

            // Check package file list
            string packageFiles = string.Empty;

            if (null != package.Files)
            {
                foreach (var file in package.Files!)
                {
                    if (File.Exists(file.Value))
                    {
                        packageFiles += file.Value + " ";
                        log.Information("Add file {file}={name} to japp package", file.Key, file.Value);
                    }
                    else
                    {
                        log.Error("Missing file {file}, can't build japp package", file.Value);

                        return 4;
                    }
                }
            }

            // Pull container images
            if (null != package.Containers)
            {
                foreach (var container in package.Containers)
                {
                    string containerImage = $"{container.Registry}/{container.Image}:{container.Tag}";

                    log.Information("Pull container image {containerImage}", containerImage);

                    var pullResult = Helper.RunCommand(log, $"podman pull {containerImage}");

                    if (pullResult.returncode != 0)
                    {
                        return pullResult.returncode;
                    }
                }
            }

            // Create Dockerfile
            string dockerfile = Path.Combine(packageDir, "Dockerfile");

            StringBuilder docker = new();
            docker.AppendLine("FROM scratch");
            docker.AppendFormat("COPY package.yml README.md logo.png {0}/\n", packageFiles);
            docker.AppendFormat("LABEL japp=\"{0}\"\n", package.ApiVersion);
            docker.Append("CMD [\"/jappinfo\"]"); // TODO build jappinfo binary

            File.WriteAllText(dockerfile, docker.ToString());
            log.Debug("{dockerfile}:\n{docker}", dockerfile, docker.ToString());

            // Build japp package with podman build
            string tag = $"{myConfig.Registry}/{package.Name}:{package.Version}";

            log.Information("Build japp package {tag}", tag);

            var buildResult = Helper.RunCommand(log, $"podman build -t {tag} {packageDir}");

            // Delete Dockerfile
            File.Delete(dockerfile);

            if (buildResult.returncode != 0)
            {
                return buildResult.returncode;
            }

            return 0;
        }
    }
}
