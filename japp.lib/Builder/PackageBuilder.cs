using japp.lib.Models;

namespace japp.lib.Builder
{
    public class PackageBuilder
    {
        public PackageModel? package { get; internal set; }

        public static PackageBuilder Initialize(string name, string version, string description = "")
        {
            var builder = new PackageBuilder()
            {
                package = new()
                {
                    ApiVersion = "japp/v1",
                    Name = name,
                    Version = version,
                    Description = description,
                    Install = new()
                    {
                        Tasks = new()
                    },
                }
            };

            return builder;
        }
    }
}
