using System.Net;
using Ardumine.Module.Base;

namespace Ardumine.Module.AFCPClientTest;
public class AFCPClientTestImplement : AFCPClientTestInterface, BaseImplement
{

    public string Path { get; set; }
    public Guid guid { get; set; }
    public Logger logger { get; set; }

    public void Prepare()
    {
        logger.LogI("Preparing Lidar...");
    }

    public void Start()
    {
        logger.LogI("Starting Lidar...");
    }

    public void EndStop()
    {
        logger.LogI("Stoping Lidar...");
    }

    public void Connect(IPEndPoint iPEndPoint)
    {
        throw new NotImplementedException();
    }
}