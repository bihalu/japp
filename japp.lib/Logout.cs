using japp.lib.Models;
using Serilog;

namespace japp.lib
{
    internal class Logout
    {
        public int Execute(ILogger log, ConfigModel myConfig)
        {
            string registry = myConfig.Registry;

            var logoutResult = Helper.RunCommand(log, $"podman logout {registry}");

            if (logoutResult.returncode == 0)
            {
                log.Information("{result}", logoutResult.stdout);
            }

            return logoutResult.returncode;
        }
    }
}
