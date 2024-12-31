
using Ardumine.Modules.Module1;

namespace Ardumine.Modules.Module2;
public class Module2Implement : Kernel.Modules.Base.BaseImplement, IModule2
{

    public override void Prepare()
    {
    }

    public override void Start()
    {
        Logger.LogI("Starting...");

        var interfaceMod1 = moduleManager.GetConector<IModule1>("/modules/mod1");
        interfaceMod1.CoolFunc();
        Logger.LogI("Module2 started!");

    }

    public override void EndStop()
    {
    }

}