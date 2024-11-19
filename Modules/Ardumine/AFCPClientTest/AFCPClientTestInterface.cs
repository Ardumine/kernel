using System.Net;

namespace Ardumine.Module.AFCPClientTest;

//This file must be shared between the implement and the conector
public interface AFCPClientTestInterface : IModuleInterface
{
    public void Connect(IPEndPoint iPEndPoint);
    public void Disconnect(IPEndPoint iPEndPoint);

}
