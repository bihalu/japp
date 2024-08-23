using ICSharpCode.SharpZipLib.Tar;
using japp.lib.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace japp.lib;

public class Japp : IJapp
{
    private readonly ILogger log;
    private readonly IConfiguration config;
    private readonly ConfigModel myConfig;

    public Japp(ILogger log, IConfiguration config)
    {
        this.log = log;
        this.config = config;
        myConfig = Helper.BindConfig(config);
    }

    public int Create(string packageFolder)
    {
        // Package folder
        if (string.IsNullOrWhiteSpace(packageFolder))
        {
            packageFolder = Directory.GetCurrentDirectory();
        }

        if (!Directory.Exists(packageFolder))
        {
            Directory.CreateDirectory(packageFolder);
        }

        // package.yml
        string packageYml = Path.Combine(packageFolder, "package.yml");

        if (File.Exists(packageYml))
        {
            log.Warning("File {package} already exists, don't overwrite", packageYml);
        }
        else
        {
            log.Debug("Create file {package}", packageYml);
            using var stream = typeof(Japp).Assembly.GetManifestResourceStream("japp.lib.Template.package.yml")!;
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            File.WriteAllText(packageYml, streamReader.ReadToEnd());
        }

        // logo.png
        string logoPng = Path.Combine(packageFolder, "logo.png");

        if (File.Exists(logoPng))
        {
            log.Warning("File {logo} already exists, don't overwrite", logoPng);
        }
        else
        {
            log.Debug("Create file {logo}", logoPng);
            using var stream = typeof(Japp).Assembly.GetManifestResourceStream("japp.lib.Template.logo.png")!;
            var length = stream.Length;

            if (length <= int.MaxValue)
            {
                var result = new byte[length];
                var bytesRead = stream.Read(result, 0, (int)length);
                if (bytesRead == length)
                {
                    File.WriteAllBytes(logoPng, result);
                }
            }
        }

        // README.md
        string readmeMd = Path.Combine(packageFolder, "README.md");

        if (File.Exists(readmeMd))
        {
            log.Warning("File {readme} already exists, don't overwrite", readmeMd);
        }
        else
        {
            log.Debug("Create file {readme}", readmeMd);
            using var stream = typeof(Japp).Assembly.GetManifestResourceStream("japp.lib.Template.README.md")!;
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            File.WriteAllText(readmeMd, streamReader.ReadToEnd());
        }

        return 0;
    }

    public int Build(string packageFolder)
    {
        // Check package folder
        if (string.IsNullOrWhiteSpace(packageFolder))
        {
            packageFolder = Directory.GetCurrentDirectory();
        }

        if (!Directory.Exists(packageFolder)) 
        {
            log.Error("Missing folder {folder}, can't build japp package", packageFolder);

            return 1;
        }

        // Read package.yml
        PackageModel package;
        string packageYml = Path.Combine(packageFolder, "package.yml");

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
                    log.Debug("Add file {file}={name} to japp package", file.Key, file.Value);
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
                var pullResult = Helper.RunCommand(log, $"podman pull {containerImage}");

                if (pullResult.returncode != 0) 
                {
                    log.Error("Error pulling image {image} => {error}", containerImage, pullResult.stderr);

                    return pullResult.returncode;
                }
            }
        }

        // Create Dockerfile
        string dockerfile = Path.Combine(packageFolder, "Dockerfile");

        StringBuilder docker = new();
        docker.AppendLine("FROM scratch");
        docker.AppendFormat("COPY package.yml README.md logo.png {0}/\n", packageFiles);
        docker.AppendFormat("LABEL japp=\"{0}\"\n", package.ApiVersion);
        docker.Append("CMD [\"/jappinfo\"]"); // TODO build jappinfo binary

        File.WriteAllText(dockerfile, docker.ToString());
        log.Debug("{dockerfile}:\n{docker}", dockerfile, docker.ToString());
        
        // Build japp package with podman build
        string tag = $"{myConfig.Registry}/{package.Name}:{package.Version}";
        var buildResult = Helper.RunCommand(log, $"podman build -t {tag} .");

        // Delete Dockerfile
        File.Delete(dockerfile);

        if (buildResult.returncode != 0) 
        {
            log.Error("Error build image {tag} => {error}", tag, buildResult.stderr);
            return buildResult.returncode;
        }

        return 0;
    }

    public int Pull(string package, string output)
    {
        string registry = myConfig.Registry;
        string options = myConfig.TlsVerify ? "" : "--tls-verify=false "; // Workaround for self signed registry cert
        string temp = Path.Combine(myConfig.TempFolder, Guid.NewGuid().ToString("N"));

        // Pull container image
        var pullResult = Helper.RunCommand(log, $"podman pull {options}{registry}/{package}");

        if (pullResult.returncode != 0) 
        {
            string error = pullResult.stderr.Split('\n').Last();
            log.Error("{error}", error);

            return pullResult.returncode;
        }

        // Inspect container image
        var inspectResult = Helper.RunCommand(log, $"podman inspect {registry}/{package}");

        if (inspectResult.returncode != 0)
        {
            return inspectResult.returncode;
        }

        // Gather metadata: id, labels, layers, ...
        string json = inspectResult.stdout.TrimStart('[').TrimEnd('\n').TrimEnd('\r').TrimEnd(']');
        var containerImage = JsonConvert.DeserializeObject<dynamic>(json)!;
        string id = containerImage.Id;
        JObject labels = containerImage.Labels;
        JArray layers = containerImage.RootFS.Layers;

        // Check label -> japp package apiVersion
        if (null == labels || labels.Count == 0)
        {
            log.Error("Missing label japp in package {package}, Not a japp package", package);

            return 1;
        }

        string apiVersion = string.Empty;

        foreach(JProperty property in labels.Children())
        {
            log.Debug("Label: {name}={value}", property.Name, property.Value.ToString());

            // japp package must have label japp
            if (property.Name == "japp")
            {
                apiVersion = property.Value.ToString();
                log.Debug("found japp apiVersion {apiVersion}", apiVersion);
                break;
            }
        }

        if (apiVersion != "japp/v1")
        {
            log.Error("Invalid or missing japp api version in {package}, Not a japp package", package);

            return 2;
        }

        string layer = layers[0].ToString().Replace("sha256:", ""); // japp package has exactly one layer

        log.Debug("Package: name={package}", package);
        log.Debug("Package: id={id}", id);
        log.Debug("Package: apiVersion={apiVersion}", apiVersion);
        log.Debug("Package: layer={layer}", layer);

        // Save container image
        var saveResult = Helper.RunCommand(log, $"podman save --format=docker-dir {registry}/{package} --output {temp}");

        if (saveResult.returncode != 0)
        {
            return saveResult.returncode;
        }

        // Extract japp package
        if (string.IsNullOrWhiteSpace(output))
        {
            output = Directory.GetCurrentDirectory();
        }

        string sourceLayer = Path.Combine(temp, layer);
        string outputFolder = Path.Combine(output, id);
        
        using Stream inStream = File.OpenRead(sourceLayer);
        using TarArchive tarArchive = TarArchive.CreateInputTarArchive(inStream, Encoding.Default);
        tarArchive.ExtractContents(outputFolder);
        tarArchive.Close();
        inStream.Close();
        
        return 0;
    }

    public int Push(string package)
    {
        throw new NotImplementedException();
    }
}
