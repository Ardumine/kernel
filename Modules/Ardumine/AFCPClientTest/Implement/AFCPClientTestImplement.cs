using System.Net;
using System.Text;
using Ardumine.AFCP.Core.Client;
using Ardumine.Module.Base;

namespace Ardumine.Module.AFCPClientTest;
public class AFCPClientTestImplement : AFCPClientTestInterface, BaseImplement
{

    public string Path { get; set; }
    public Guid guid { get; set; }
    public Logger logger { get; set; }

    public void Prepare()
    {
    }

    public void Start()
    {
        new Thread(Main).Start();
    }

    public void EndStop()
    {
    }

    private void Main(){
        Thread.Sleep(500);
        var tcpclient = new AFCPTCPClient(IPAddress.Loopback);
        tcpclient.Connect();
        logger.LogI("Begin test AFCP client");

        tcpclient.SendData(Encoding.UTF8.GetBytes("Haro? Hibachi, Benihana, Teriyaki..."));

        var dataRead = tcpclient.ReadData();
        logger.LogI($"Received {Encoding.UTF8.GetString(dataRead)}");
        tcpclient.Close();
    }

}