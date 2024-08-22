using YamlDotNet.Serialization;

namespace japp.lib.Models;

public class PackageModel
{
    [YamlMember(Alias = "apiVersion")]
    public required string ApiVersion { get; set; }

    [YamlMember(Alias = "name")]
    public required string Name { get; set; }

    [YamlMember(Alias = "description")]
    public required string Description { get; set; }

    [YamlMember(Alias = "version")]
    public required string Version { get; set; }

    [YamlMember(Alias = "files")]
    public Dictionary<string, string>? Files { get; set; }

    [YamlMember(Alias = "containers")]
    public List<Container>? Containers { get; set; }

    //install:
    //  tasks:
    //  - name: abc
    //    command: ""
    //update:
    //  tasks:
    //  - name: abc
    //    command: ""
    //delete:
    //  tasks:
    //  - name: abc
    //    command: ""
}

public record Container
{
    [YamlMember(Alias = "registry")]
    public required string Registry { get; set;}

    [YamlMember(Alias = "image")]
    public required string Image { get; set; }

    [YamlMember(Alias = "tag")]
    public required string Tag { get; set; }
}


