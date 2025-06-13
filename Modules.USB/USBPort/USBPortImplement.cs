using Kernel.Modules.Base;

namespace Modules.USB.USBPort;

public class USBPortImplement : BaseImplement, IUSBPort
{

    public override void Prepare()
    {
    }

    public override void Start()
    {
        logger.LogI("USB port started!");
    }

    public void SetName(string name)
    {
        logger.LogI($"The USB port has the new name '{name}'");
    }
    public override void EndStop()
    {

    }

    public override void Delete()
    {

    }

}


