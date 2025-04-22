
using Ardumine.Modules.Module1;

namespace Ardumine.Modules.Module2;
public class Module2Implement : Kernel.Modules.Base.BaseImplement, IModule2
{

    public override void Prepare()
    {
    }

    public override void Start()
    {
        logger.LogI("Running the CoolFunc...");

        //Gets the interface to the module1
        var interfaceMod1 = GetModuleInterface<IModule1>("/modules/mod1");

        interfaceMod1?.CoolFunc();
        logger.LogOK("Module2 started!");
    }

    public override void EndStop()
    {
    }

}