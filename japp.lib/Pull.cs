using ICSharpCode.SharpZipLib.Tar;
using japp.lib.Models;
using Serilog;
using System.Text;

namespace japp.lib
{
    internal class Pull
    {
        public int Execute(ILogger log, ConfigModel myConfig, string packageName, string outputDir)
        {
            string registry = myConfig.Registry;
            string options = myConfig.TlsVerify ? "" : "--tls-verify=false "; // Workaround for self signed registry cert
            string tempDir = Path.Combine(myConfig.TempDir, Guid.NewGuid().ToString("N"));

            // Pull container image
            var pullResult = Helper.RunCommand(log, $"podman pull {options}{registry}/{packageName}");

            if (pullResult.returncode != 0)
            {
                return pullResult.returncode;
            }

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

            // Extract japp package
            if (string.IsNullOrWhiteSpace(outputDir))
            {
                outputDir = Directory.GetCurrentDirectory();
            }

            string sourceLayer = Path.Combine(tempDir, validateResult.layer);
            string destinationDir = Path.Combine(outputDir, validateResult.id);

            using Stream inputStream = File.OpenRead(sourceLayer);
            using TarArchive tarArchive = TarArchive.CreateInputTarArchive(inputStream, Encoding.Default);
            tarArchive.ExtractContents(destinationDir);
            tarArchive.Close();
            inputStream.Close();

            log.Information("Package {packageName} pulled to {destinationDir}", packageName, destinationDir);

            return 0;
        }
    }
}
