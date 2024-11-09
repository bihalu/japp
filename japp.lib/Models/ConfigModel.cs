namespace japp.lib.Models;

public class ConfigModel
{
    public string Registry { get; set; }

    public string TempDir { get; set; }

    public bool Cleanup { get; set; }

    public bool TlsVerify { get; set; }

    public List<AliasModel> Aliases { get; set; }

    public ConfigModel()
    {
        //Registry = "docker.io";
        Registry = "192.168.178.59:5000";

        TempDir = Path.Combine(Path.GetTempPath(), "japp");
        if (!Directory.Exists(TempDir))
        {
            Directory.CreateDirectory(TempDir);
        };

        Cleanup = false;

        TlsVerify = false;

        Aliases = 
        [ 
            new AliasModel() { Alias = "cowsay", Command = "podman run --rm rancher/cowsay", MountDir = "" },
            new AliasModel() { Alias = "figlet", Command = "podman run --rm hairyhenderson/figlet", MountDir = "" } 
        ];
    }
}
