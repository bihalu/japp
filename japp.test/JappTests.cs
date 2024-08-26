using japp.lib;
using japp.lib.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Task = japp.lib.Models.Task;
using japp.lib.Builder;

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
            string? packageDir = null;
            var japp = new Japp(log, config);

            // Act
            japp.Create(packageDir!);

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

        [Fact]
        public void SerializePackage01()
        {
            // Arrange
            PackageModel package = new()
            {
                ApiVersion = "japp/v1",
                Name = "japp/example",
                Version = "1.0.0",
                Description = "Japp example package",
                Files = new()
                {
                    { "foo", "bar" },
                    { "bar", "foo" },
                },
                Containers = new()
                {
                    new Container()
                    {
                        Registry = "docker.io",
                        Image = "library/hello-world",
                        Tag = "latest"
                    },
                },
                Install = new()
                {
                    Tasks = new List<Task>()
                    {
                        {
                            new Task()
                            {
                                Name = "Test",
                                Description = "Test",
                                Command = "echo Test",
                            }
                        },
                    },
                },
            };

            // Act
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(package);

            // Assert
            Assert.Equal(File.ReadAllText("package01.yml"), yaml);
        }

        [Fact]
        public void SerializePackage02()
        {
            // Arrange
            PackageModel package = new()
            {
                ApiVersion = "japp/v1",
                Name = "japp/example",
                Version = "1.0.0",
                Description = "Japp example package",
                Files = new()
                {
                    { "foo", "bar" },
                    { "bar", "foo" },
                },
                Containers = new()
                {
                    new Container()
                    {
                        Registry = "docker.io",
                        Image = "library/hello-world",
                        Tag = "latest"
                    },
                    new Container()
                    {
                        Registry = "docker.io",
                        Image = "library/nginx",
                        Tag = "latest"
                    },
                },
                Install = new()
                {
                    Tasks = new List<Task>()
                    {
                        {
                            new Task()
                            {
                                Name = "Task1",
                                Description = "Task1",
                                Command = "echo Task1",
                            }
                        },
                        {
                            new Task()
                            {
                                Name = "Task2",
                                Description = "Task2",
                                Command = "echo Task2",
                            }
                        },
                    },
                },
                Update = new()
                {
                    Tasks = new List<Task>()
                    {
                        {
                            new Task()
                            {
                                Name = "UpdateTask1",
                                Description = "UpdateTask1",
                                Command = "echo update task1",
                            }
                        },
                        {
                            new Task()
                            {
                                Name = "UpdateTask2",
                                Description = "UpdateTask2",
                                Command = "echo update task2",
                            }
                        },
                    },
                },
                Delete = new()
                {
                    Tasks = new List<Task>()
                    {
                        {
                            new Task()
                            {
                                Name = "DeleteTask1",
                                Description = "DeleteTask1",
                                Command = "echo delete task1",
                            }
                        },
                        {
                            new Task()
                            {
                                Name = "DeleteTask2",
                                Description = "DeleteTask2",
                                Command = "echo delete task2",
                            }
                        },
                    },
                },
            };

            // Act
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(package);

            // Assert
            Assert.Equal(File.ReadAllText("package02.yml"), yaml);
        }

        [Fact]
        public void DeserializePackage03()
        {
            // Arrange
            var yaml = File.ReadAllText("package03.yml");

            // Act
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var package = deserializer.Deserialize<PackageModel>(yaml);

            // Assert
            Assert.Equal("japp/v1", package.ApiVersion);
            Assert.Equal("japp/example", package.Name);
            Assert.Equal("1.0", package.Version);
            Assert.Equal("Japp package template", package.Description);
            Assert.Null(package.Files);
        }

        [Fact]
        public void PackageBuilderPackage01()
        {
            // Arrange
            var expectedYaml = File.ReadAllText("package01.yml");

            // Act
            var builder = PackageBuilder.Initialize("japp/example", "1.0.0", "Japp example package");
            builder.AddFile(name: "foo", path: "bar");
            builder.AddFile(name: "bar", path: "foo");
            builder.AddContainer(registry: "docker.io", image: "library/hello-world", tag: "latest");
            builder.AddInstallTask(name: "Test", command: "echo Test", description: "Test");

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var actualYaml = serializer.Serialize(builder.package);

            // Assert
            Assert.Equal(expectedYaml, actualYaml);
        }

        [Fact]
        public void PackageBuilderPackage02()
        {
            // Arrange
            var expectedYaml = File.ReadAllText("package02.yml");

            // Act
            var builder = PackageBuilder.Initialize("japp/example", "1.0.0", "Japp example package");
            builder.AddFile(name: "foo", path: "bar");
            builder.AddFile(name: "bar", path: "foo");
            builder.AddContainer(registry: "docker.io", image: "library/hello-world", tag: "latest");
            builder.AddContainer(registry: "docker.io", image: "library/nginx", tag: "latest");
            builder.AddInstallTask(name: "Task1", command: "echo Task1", description: "Task1");
            builder.AddInstallTask(name: "Task2", command: "echo Task2", description: "Task2");
            builder.AddUpdateTask(name: "UpdateTask1", command: "echo update task1", description: "UpdateTask1");
            builder.AddUpdateTask(name: "UpdateTask2", command: "echo update task2", description: "UpdateTask2");
            builder.AddDeleteTask(name: "DeleteTask1", command: "echo delete task1", description: "DeleteTask1");
            builder.AddDeleteTask(name: "DeleteTask2", command: "echo delete task2", description: "DeleteTask2");

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var actualYaml = serializer.Serialize(builder.package);

            // Assert
            Assert.Equal(expectedYaml, actualYaml);
        }
    }
}
