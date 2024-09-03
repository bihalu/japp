using japp.lib.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using japp.lib.Builder;

namespace japp.lib
{
    internal class Validate
    {
        public (int returncode, string id, string layer) Image(ILogger log, ConfigModel myConfig, string packageName)
        {
            string registry = myConfig.Registry;
            string id = string.Empty;
            string layer = string.Empty;

            // Inspect container image
            var inspectResult = Helper.RunCommand(log, $"podman inspect {registry}/{packageName}");

            if (inspectResult.returncode != 0)
            {
                return (inspectResult.returncode, id, layer);
            }

            // Gather metadata: id, labels, layers, ...
            string json = inspectResult.stdout.TrimStart('[').TrimEnd('\n').TrimEnd('\r').TrimEnd(']');
            var containerImage = JsonConvert.DeserializeObject<dynamic>(json)!;
            id = containerImage.Id;
            JObject labels = containerImage.Labels;
            JArray layers = containerImage.RootFS.Layers;

            // Check label -> japp package apiVersion
            if (null == labels || labels.Count == 0)
            {
                log.Error("Missing label japp in package {package}, Not a japp package", packageName);

                return (1, id, layer);
            }

            string apiVersion = string.Empty;

            foreach (JProperty property in labels.Children())
            {
                log.Debug("Label: {name}={value}", property.Name, property.Value.ToString());

                // japp package must have label japp
                if (property.Name == "japp")
                {
                    apiVersion = property.Value.ToString();
                    log.Debug("Found japp apiVersion: {apiVersion}", apiVersion);
                    break;
                }
            }

            if (apiVersion != "japp/v1")
            {
                log.Error("Invalid api version in {package}, Not a japp package", packageName);

                return (2, id, layer);
            }

            layer = layers[0].ToString().Replace("sha256:", ""); // japp package has exactly one layer

            log.Debug("Package: name={package}", packageName);
            log.Debug("Package: id={id}", id);
            log.Debug("Package: apiVersion={apiVersion}", apiVersion);
            log.Debug("Package: layer={layer}", layer);

            return (0, id, layer);
        }

        public (int returncode, PackageModel package) Source(ILogger log, ConfigModel myConfig, string packageDir)
        {
            // Initialize dummy package
            var builder = PackageBuilder.Initialize(name: "dummy", version: "0.0.0");
            PackageModel package = builder.package!;

            // Read package.yml
            string packagePath = Path.Combine(packageDir, "package.yml");

            if (!File.Exists(packagePath))
            {
                log.Error("Missing file {package} in japp package", packagePath);

                return (2, package);
            }
            else
            {
                var yaml = File.ReadAllText(packagePath);
                log.Debug("{packagePath}:\n{yaml}", packagePath, yaml);

                try
                {
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    package = deserializer.Deserialize<PackageModel>(yaml);
                }
                catch (Exception exception)
                {
                    log.Error("Invalid japp package {package} => {error}", packagePath, exception.Message);

                    return (3, package);
                }
            }

            // Check api version
            if (package.ApiVersion != "japp/v1")
            {
                log.Error("Invalid api version in {package}, Not a japp package", packagePath);

                return (4, package);
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
                        log.Debug("Add file {file}={name} to japp package", file.Key, file.Value);
                    }
                    else
                    {
                        log.Error("Missing file {file} in japp package", file.Value);

                        return (5, package);
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
                        return (pullResult.returncode, package);
                    }
                }
            }

            return (0, package);
        }
    }
}
