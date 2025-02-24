
namespace Ardumine.Modules.Module1;
public class Module1Implement : Kernel.Modules.Base.BaseImplement, IModule1
{
    public override void Prepare()
    {
    }

    public override void Start()
    {
        Logger.LogI("Module1 Started!");
    }
    public override void EndStop()
    {

    }

    public void CoolFunc()
    {
        Logger.LogI("CoolFunc was ran!");
    }
}