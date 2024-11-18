
namespace Ardumine.Module.Base;

public interface Module
{
    public Guid guid { get; set; }
    public string Path { get; set; }
    public ModuleDescription description { get; }
}
public interface ModuleDescription
{
    public string Name { get; }
    public string FriendlyName { get; }
    public string Version { get; }
    public string NameImplement { get; }
    public string NameConector { get; }
    public string NameBase { get; }
}