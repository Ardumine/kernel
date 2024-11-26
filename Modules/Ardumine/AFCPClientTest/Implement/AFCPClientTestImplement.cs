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

    private void Main()
    {
        Thread.Sleep(100);//Let the rest of the kernel boot
        var AFCPClient = new AFCPTCPClient(IPAddress.Loopback, true);
        AFCPClient.Name = "Client";
        
        logger.LogI("A conectar...");
        bool stat = AFCPClient.Connect();

        logger.LogI("Client connected!");

        if (!stat)
        {
            logger.LogW("Client: wrong password!");
            return;
        }
        Thread.Sleep(200);

        logger.LogI("Begin test AFCP client");

        logger.LogI("Sending: Haro? Hibachi, Benihana, Teriyaki...");
        AFCPClient.rawComProt.SendData(280, Encoding.UTF8.GetBytes("Haro? Hibachi, Benihana, Teriyaki..."));

        var dataRead = AFCPClient.ReadChannelData(280);
        logger.LogI($"Received {Encoding.UTF8.GetString(dataRead)}");//Nagasaki, Okinawa, Hokkaido...Yokohama
        AFCPClient.Ask(250, [0, 0]);


        Thread.Sleep(1000);
        AFCPClient.Close();
    }

}