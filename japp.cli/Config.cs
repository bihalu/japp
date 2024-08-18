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

        if (false == Directory.Exists(userDir))
        {
            Directory.CreateDirectory(userDir);
        };

        if (false == File.Exists(userConfig))
        {
            Save(new Config());
        }

        return userConfig;
    }

    public static bool Save(Config config)
    {
        var newConfig = JsonSerializer.Serialize<Config>(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Config.GetPath(), newConfig);

        return true;
    }

    public static bool Reset()
    {
        return Save(new Config());
    }    
}
