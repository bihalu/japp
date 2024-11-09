namespace japp.lib.Models;

public class AliasModel
{
    public string Alias { get; set; }

    public string Command { get; set; }

    public string MountDir { get; set; }

    public AliasModel()
    {
        Alias = "";

        Command = "";

        MountDir = "";
    }
}
