using Ardumine.Module.Base;

namespace Ardumine.Module.AFCPClientTest;

class AFCPClientTestDescription : ModuleDescription
{
    public string Name => GetType().FullName;

    public string FriendlyName => "AFCPClientTest";

    public string Version => "0.1.0";

    public string NameImplement => typeof(AFCPClientTestImplement).FullName;

    public string NameConector => typeof(AFCPClientTestConector).FullName;

    public string NameBase  => typeof(AFCPClientTest).FullName; 

}
