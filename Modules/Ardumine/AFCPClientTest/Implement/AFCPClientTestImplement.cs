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
        Thread.Sleep(100);//Let the rest of the kernel boot
        var tcpclient = new AFCPTCPClient(IPAddress.Loopback);
        tcpclient.Connect();
        logger.LogI("Begin test AFCP client");

        logger.LogI("Sending: Haro? Hibachi, Benihana, Teriyaki...");
        tcpclient.SendData(Encoding.UTF8.GetBytes("Haro? Hibachi, Benihana, Teriyaki..."));

        var dataRead = tcpclient.ReadData();
        logger.LogI($"Received {Encoding.UTF8.GetString(dataRead)}");//Nagasaki, Okinawa, Hokkaido...Yokohama
        tcpclient.Close();
    }

}