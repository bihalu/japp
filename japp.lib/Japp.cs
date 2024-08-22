using japp.lib.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
        if (!Directory.Exists(packageFolder)) Directory.CreateDirectory(packageFolder);

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
                if (bytesRead == length) File.WriteAllBytes(logoPng, result);
            }
        }

        // docs/README.md
        string docsFolder = Path.Combine(packageFolder, "docs");
        string docsReadme = Path.Combine(packageFolder, "docs", "README.md");
        if (File.Exists(docsReadme))
        {
            log.Warning("File {logo} already exists, don't overwrite", docsReadme);
        }
        else
        {
            if (!Directory.Exists(docsFolder))
            {
                log.Debug("Create folder {docs}", docsFolder);
                Directory.CreateDirectory(docsFolder);
            }

            log.Debug("Create file {readme}", docsReadme);
            using var stream = typeof(Japp).Assembly.GetManifestResourceStream("japp.lib.Template.docs.README.md")!;
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            File.WriteAllText(docsReadme, streamReader.ReadToEnd());
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

        if (false)
        {
            package = new()
            {
                ApiVersion = "japp/v1_alpha",
                Name = "jappexample",
                Description = "Japp package template",
                Version = "1.0",
                Files = new Dictionary<string, string>()
                {
                    {"readme", "README.md"}
                },
                Containers = new List<Container>()
                {
                    new Container()
                    {
                        Registry = "docker.io",
                        Image = "rancher/cowsay",
                        Tag = "latest",
                    }
                }
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(package);
            log.Debug("Yaml:\n{yaml}", yaml);
        }

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
                log.Error("Invalid japp package {package}, {error}", packageYml, exception.Message);
                return 3;
            }
        }

        // check files list
        if (null != package.Files)
        {
            foreach (var file in package.Files!)
            {
                if (File.Exists(file.Value))
                {
                    log.Debug("Add file {file}={name} to japp package", file.Key, file.Value);
                }
                else
                {
                    log.Error("Missing file {file}, can't build japp package", file.Value);
                    return 4;
                }
            }
        }

        // pull container images
        // create Dockerfile
        // -> set annotation japp apiversion
        // -> copy package.yml docs/README.md logo.png and all files
        // podman build

        return 0;
    }

    public int Pull(string package, string output)
    {
        string registry = myConfig.Registry;
        string options = myConfig.TlsVerify ? "" : "--tls-verify=false ";
        string temp = Path.Combine(myConfig.TempFolder, Guid.NewGuid().ToString("N"));

        // Pull container image
        var pullResult = Helper.RunCommand(log, $"podman pull {options}{registry}/{package}");
        if (pullResult.returncode != 0) return pullResult.returncode;

        // Inspect container image
        var inspectResult = Helper.RunCommand(log, $"podman inspect {registry}/{package}");
        if (inspectResult.returncode != 0) return inspectResult.returncode;

        // Gather metadata
        string json = inspectResult.stdout.TrimStart('[').TrimEnd('\n').TrimEnd('\r').TrimEnd(']');
        var containerImage = JsonConvert.DeserializeObject<dynamic>(json)!;
        string id = containerImage.Id;
        var annotations = containerImage.Annotations;
        string graphDriverName = containerImage.GraphDriver.Name;
        string upperDir = containerImage.GraphDriver.Data.UpperDir;

        // Check annotations -> japp package version

        // Save container image
        var saveResult = Helper.RunCommand(log, $"podman save --format=docker-dir {registry}/{package} --output {temp}");
        return saveResult.returncode;

        // Copy japp package output
    }

    public int Push(string package)
    {
        throw new NotImplementedException();
    }
}
