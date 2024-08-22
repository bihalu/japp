using japp.lib.Models;
using Serilog;

namespace japp.lib;

public interface IJapp
{
    public int Create(string packageFolder);

    public int Build(string packageFolder);

    public int Push(string package);

    public int Pull(string package, string output);
}
