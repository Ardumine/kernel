using Kernel.Modules.Base;

namespace Ardumine.Modules.Module2;

public class Module2Description : ModuleDescription
{
    public string Name => "Module2";

    public string FriendlyName => "Module 2";

    public string Version => "0.1.0";

    public Type ImplementType => typeof(Module2Implement);
    public Type BaseType => typeof(Module2);

    public Type InterfaceType => typeof(IModule2);
}
