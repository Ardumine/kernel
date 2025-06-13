using Kernel.Modules.Base;
using Modules.USB.USBPort;

namespace Modules.USB.USBController;

public class USBControllerImplement : Kernel.Modules.Base.BaseImplement, IUSBController
{

    private List<IUSBPort> _usbPorts = new List<IUSBPort>();
    public override void Prepare()
    {
        
    }

    public override void Start()
    {
        logger.LogI("USB controller started!");

        logger.LogI("Detecting USB Ports...");

        //Try to detect the existing USB Ports
        var portNames = new List<string>()
        {
            "Port1",
            "Port2",
            "Port3"
        };

        logger.LogI($"Detected {portNames.Count} ports!");

        //Create an sub-module to each port
        foreach (var portName in portNames)
        {
            var portMod = CreateSubModule<IUSBPort>($"/modules/USBController/{portName}", "USBPort");
            if (portMod == null)
                throw new NullReferenceException($"The return of the creation of the submodule was null");
            portMod.SetName(portName);

            _usbPorts.Add(portMod);
        }


    }

    public override void EndStop()
    {
        foreach (var port in _usbPorts)
        {
            StopSubModule(port.MPath);
        }
    }
    public override void Delete()
    {
       foreach (var port in _usbPorts)
        {
            DeleteSubModule(port.MPath);
        }
    }

}


