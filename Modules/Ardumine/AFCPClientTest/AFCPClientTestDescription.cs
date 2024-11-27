using Ardumine.Module.Base;

namespace Ardumine.Module.AFCPClientTest;

class AFCPClientTestDescription : ModuleDescription
{
    public string Name => "AFCPClientTest";

    public string FriendlyName => "AFCP Client Test";

    public string Version => "0.1.0";

    public string NameImplement => typeof(AFCPClientTestImplement).FullName;

    public string NameBase => typeof(AFCPClientTest).FullName; 

}
