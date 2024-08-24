using japp.lib;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace japp.test
{
    public class JappTests : IClassFixture<Setup>
    {
        private readonly ILogger log;
        private readonly IConfiguration config;

        public JappTests(Setup setup)
        {
            log = setup.log;
            config = setup.config;
        }

        [Fact]
        public void CreateWithoutPackageDir()
        {
            // Arrange
            string packageDir = null;
            var japp = new Japp(log, config);

            // Act
            japp.Create(packageDir);

            // Assert
            Assert.True(File.Exists("package.yml"));
            Assert.True(File.Exists("logo.png"));
            Assert.True(File.Exists("README.md"));
        }

        [Fact]
        public void CreateWithPackageDir()
        {
            // Arrange
            string packageDir = "mypackage";
            var japp = new Japp(log, config);

            // Act
            japp.Create(packageDir);

            // Assert
            Assert.True(File.Exists("mypackage/package.yml"));
            Assert.True(File.Exists("mypackage/logo.png"));
            Assert.True(File.Exists("mypackage/README.md"));
        }
    }
}
