
namespace Ardumine.Module.Base;

public interface Module
{
    public string Path { get; set; }
    public ModuleDescription description { get; }
}
public interface ModuleDescription
{
    public string Name { get; }
    public string FriendlyName { get; }
    public string Version { get; }
    public string NameImplement { get; }
    public string NameInterfacer { get; }
    public string NameBase { get; }
}