using japp.lib.Models;
using Serilog;
using System.Text;

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

            // Validate package source
            var validateResult = new Validate().Source(log, myConfig, packageDir);
            if (validateResult.returncode != 0)
            {
                return validateResult.returncode;
            }

            // Package
            var package = validateResult.package;
            string packageFiles = string.Empty;

            if (null != package.Files && package.Files.Count > 0)
            {
                packageFiles = string.Join(' ', package.Files.Select(f => f.Value).ToArray());
            }

            // Create Dockerfile
            string dockerfile = Path.Combine(packageDir, "Dockerfile");

            StringBuilder docker = new();
            docker.AppendLine("FROM scratch");
            docker.AppendFormat("COPY package.yml README.md logo.png {0} /\n", packageFiles);
            docker.Append("COPY --chmod=755 jappinfo /\n");
            docker.AppendFormat("LABEL japp=\"{0}\"\n", package.ApiVersion);
            docker.Append("CMD [\"./jappinfo\"]");

            File.WriteAllText(dockerfile, docker.ToString());
            log.Debug("{dockerfile}:\n{docker}", dockerfile, docker.ToString());

            // Create jappinfo
            string jappinfo = Path.Combine(packageDir, "jappinfo");

            using var stream = typeof(Japp).Assembly.GetManifestResourceStream("japp.lib.Template.jappinfo")!;
            var length = stream.Length;

            if (length <= int.MaxValue)
            {
                var result = new byte[length];
                var bytesRead = stream.Read(result, 0, (int)length);
                if (bytesRead == length)
                {
                    File.WriteAllBytes(jappinfo, result);
                }
            }

            // Build japp package with podman build
            string tag = $"{myConfig.Registry}/{package.Name}:{package.Version}";

            log.Information("Build japp package {tag}", tag);

            var buildResult = Helper.RunCommand(log, $"podman build -t {tag} {packageDir}");

            // Delete Dockerfile
            File.Delete(dockerfile);

            // Delete jappinfo
            File.Delete(jappinfo);

            if (buildResult.returncode != 0)
            {
                return buildResult.returncode;
            }

            return 0;
        }
    }
}
