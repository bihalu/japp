namespace japp.lib.Builder
{
    public static class PackageBuilderExtensions
    {
        public static PackageBuilder AddFile(this PackageBuilder builder, string name, string path)
        {
            if (builder.package!.Files == null)
            {
                builder.package.Files = new();
            }

            builder.package.Files.Add(name, path);

            return builder;
        }

        public static PackageBuilder AddContainer(this PackageBuilder builder, string registry, string image, string tag)
        {
            if (builder.package!.Containers == null)
            {
                builder.package.Containers = new();
            }

            builder.package.Containers.Add(new Models.Container()
            {
                Registry = registry,
                Image = image,
                Tag = tag
            });

            return builder;
        }

        public static PackageBuilder AddInstallTask(this PackageBuilder builder, string name, string command, string description = "")
        {
            builder.package!.Install.Tasks.Add(new Models.Task()
            {
                Name = name,
                Command = command,
                Description = description
            });

            return builder;
        }

        public static PackageBuilder AddUpdateTask(this PackageBuilder builder, string name, string command, string description = "")
        {
            if (builder.package!.Update == null)
            {
                builder.package.Update = new()
                {
                    Tasks = new()
                };
            }

            builder.package!.Update.Tasks.Add(new Models.Task()
            {
                Name = name,
                Command = command,
                Description = description
            });

            return builder;
        }

        public static PackageBuilder AddDeleteTask(this PackageBuilder builder, string name, string command, string description = "")
        {
            if (builder.package!.Delete == null)
            {
                builder.package.Delete = new()
                {
                    Tasks = new()
                };
            }

            builder.package!.Delete.Tasks.Add(new Models.Task()
            {
                Name = name,
                Command = command,
                Description = description
            });

            return builder;
        }
    }
}
