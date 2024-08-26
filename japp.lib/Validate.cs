using japp.lib.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

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
                log.Error("Invalid or missing japp api version in {package}, Not a japp package", packageName);

                return (2, id, layer);
            }

            layer = layers[0].ToString().Replace("sha256:", ""); // japp package has exactly one layer

            log.Debug("Package: name={package}", packageName);
            log.Debug("Package: id={id}", id);
            log.Debug("Package: apiVersion={apiVersion}", apiVersion);
            log.Debug("Package: layer={layer}", layer);

            return (0, id, layer);
        }

        public int Source(ILogger log, ConfigModel myConfig, string dir)
        {
            string registry = myConfig.Registry;
            return 0;
        }
    }
}
