using Newtonsoft.Json;

namespace japp.cli;

public class Config
{
    public string Registry { get; set; }

    public Config()
    {
        //Registry = "docker.io";
        Registry = "192.168.178.59:5000";
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
            var newConfig = JsonConvert.SerializeObject(new Config());
            File.WriteAllText(userConfig, newConfig);
        }

        return userConfig;
    }

    public static bool Save(Config config)
    {
        var newConfig = JsonConvert.SerializeObject(config);
        File.WriteAllText(Config.GetPath(), newConfig);

        return true;
    }

    public static bool Reset()
    {
        return Save(new Config());
    }
}
