using System.Text.Json;

namespace japp.cli;

public class Config
{
    public string Registry { get; set; }

    public Config()
    {
        Registry = "https://docker.io";
    }

    public static string GetPath()
    {
        var userProfile = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        var userDir = Path.Combine(userProfile.FullName, ".japp");
        var userConfig = Path.Combine(userDir, "config.json");

        if (!Directory.Exists(userDir))
        {
            Directory.CreateDirectory(userDir);
        };

        if (!File.Exists(userConfig))
        {
            var newConfig = JsonSerializer.Serialize<Config>(new Config(), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(userConfig, newConfig);
        }

        return userConfig;
    }

    public static bool Save(Config config)
    {
        var newConfig = JsonSerializer.Serialize<Config>(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Config.GetPath(), newConfig);

        return true;
    }
}
