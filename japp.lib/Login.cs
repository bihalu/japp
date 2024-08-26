using japp.lib.Models;
using Serilog;

namespace japp.lib
{
    internal class Login
    {
        public int Execute(ILogger log, ConfigModel myConfig, string username, string password)
        {
            string registry = myConfig.Registry;

            var loginResult = Helper.RunCommand(log, $"podman login {registry} --username {username} --password {password}");

            if (loginResult.returncode == 0)
            {
                log.Information("{result}", loginResult.stdout);
            }

            return loginResult.returncode;
        }
    }
}
