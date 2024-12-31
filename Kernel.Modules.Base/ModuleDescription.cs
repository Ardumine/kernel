namespace Kernel.Modules.Base;

public interface ModuleDescription
{
    public string Name { get; }
    public string FriendlyName { get; }
    public string Version { get; }
    public Type ImplementType { get; }
    public Type BaseType { get; }
    public Type InterfaceType { get; }

}