namespace japp.lib;

public interface IJapp
{
    public int Create(string packageDir);

    public int Build(string packageDir);

    public int Pull(string packageName, string outputDir);

    public int Push(string packageName, bool retag);

    public int Login(string username, string password);

    public int Logout();

    public int Install(string packageName, string values, string packageDir);
}
