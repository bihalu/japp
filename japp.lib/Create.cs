using japp.lib.Models;
using Serilog;
using System.Text;

namespace japp.lib
{
    internal class Create
    {
        public int Execute(ILogger log, ConfigModel myConfig, string packageDir)
        {
            // Package dir
            if (string.IsNullOrWhiteSpace(packageDir))
            {
                packageDir = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(packageDir))
            {
                Directory.CreateDirectory(packageDir);
            }

            // package.yml
            string packagePath = Path.Combine(packageDir, "package.yml");

            if (File.Exists(packagePath))
            {
                log.Warning("File {package} already exists, don't overwrite", packagePath);
            }
            else
            {
                log.Information("Create file {package}", packagePath);
                using var stream = typeof(Japp).Assembly.GetManifestResourceStream("japp.lib.Template.package.yml")!;
                var streamReader = new StreamReader(stream, Encoding.UTF8);
                File.WriteAllText(packagePath, streamReader.ReadToEnd());
            }

            // logo.png
            string logoPng = Path.Combine(packageDir, "logo.png");

            if (File.Exists(logoPng))
            {
                log.Warning("File {logo} already exists, don't overwrite", logoPng);
            }
            else
            {
                log.Information("Create file {logo}", logoPng);
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
            string readmeMd = Path.Combine(packageDir, "README.md");

            if (File.Exists(readmeMd))
            {
                log.Warning("File {readme} already exists, don't overwrite", readmeMd);
            }
            else
            {
                log.Information("Create file {readme}", readmeMd);
                using var stream = typeof(Japp).Assembly.GetManifestResourceStream("japp.lib.Template.README.md")!;
                var streamReader = new StreamReader(stream, Encoding.UTF8);
                File.WriteAllText(readmeMd, streamReader.ReadToEnd());
            }

            return 0;
        }
    }
}
