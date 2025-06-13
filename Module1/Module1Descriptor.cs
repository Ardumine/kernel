using Kernel.Modules.Base;

namespace Ardumine.Modules.Module1;

public class Module1Descriptor : IModuleLoadDescriptor
{
    public string Name => "Module1";

    public string FriendlyName => "Module 1";

    public string Version => "0.1.0";

    public Type ImplementType => typeof(Module1Implement);
    public Type InterfaceType =>  typeof(IModule1);
}
