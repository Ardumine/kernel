using Kernel.Modules.Base;

namespace Ardumine.Modules.Module1;

public class Module1Description : ModuleDescription
{
    public string Name => "Module1";

    public string FriendlyName => "Module 1";

    public string Version => "0.1.0";//https://www.geeksforgeeks.org/how-to-check-if-a-given-point-lies-inside-a-polygon/

    public Type ImplementType => typeof(Module1Implement);
    public Type BaseType => typeof(Module1);

    public Type InterfaceType =>  typeof(IModule1);
}
