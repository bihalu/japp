namespace japp.lib.Models;

public class ConfigModel
{
    public string Registry { get; set; }

    public string TempFolder { get; set; }

    public bool Cleanup { get; set; }

    public ConfigModel()
    {
        //Registry = "docker.io";
        Registry = "192.168.178.59:5000";

        TempFolder = Path.Combine(Path.GetTempPath(), "japp");

        Cleanup = false;
    }
}
