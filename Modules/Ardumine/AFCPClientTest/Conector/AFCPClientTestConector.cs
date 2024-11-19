
using System.Net;

namespace Ardumine.Module.AFCPClientTest;
public class AFCPClientTestConector : IModuleInterface, AFCPClientTestInterface
{
    public string Path { get; set; }

    public int MotorSpeed => (int)ModuleHelper.GetVar(Path, System.Reflection.MethodBase.GetCurrentMethod().Name);

    public Guid guid { get; set; }

    public void Prepare()
    {
        ModuleHelper.Run(Path, System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void Start()
    {
        ModuleHelper.Run(Path, System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void EndStop()
    {
        ModuleHelper.Run(Path, System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetMotorSpeed(int speed)
    {
        ModuleHelper.Run(Path, System.Reflection.MethodBase.GetCurrentMethod().Name, speed);
    }

    public void Connect(IPEndPoint iPEndPoint)
    {
        throw new NotImplementedException();
    }
}