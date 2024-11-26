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
       // Disconnect();
    }

    private AFCPTCPClient AFCPClient;
    private void Connect()
    {
        AFCPClient = new AFCPTCPClient(IPAddress.Loopback, true);
        AFCPClient.Name = "Client";

        logger.LogI("A conectar...");
        bool stat = AFCPClient.Connect();

        logger.LogI("Client connected!");
        if (!stat)
        {
            logger.LogW("Client: wrong password!");
            return;
        }
        Thread.Sleep(50);

    }

    private void Disconnect()
    {
        Thread.Sleep(10);
        AFCPClient.Close();
    }
    private void Main()
    {
        Thread.Sleep(200);//Let the rest of the kernel boot
        Connect();
        logger.LogI("Begin test AFCP client");

        logger.LogI("Sending: Haro? Hibachi, Benihana, Teriyaki...");
        AFCPClient.rawComProt.SendData(280, Encoding.UTF8.GetBytes("Haro? Hibachi, Benihana, Teriyaki..."));

        var dataRead = AFCPClient.ReadChannelData(281);
        logger.LogI($"Received {Encoding.UTF8.GetString(dataRead)}");//Nagasaki, Okinawa, Hokkaido...Yokohama

        for (int i = 0; i < 4; i++)
        {
            var aa = AFCPClient.Ask(250, [0, 0, 2]);
            for (int a = 0; a < aa.Length; a++)
            {
                Console.Write(aa[a]);
            }
            Console.WriteLine();
            Thread.Sleep(100);
        }

        //logger.LogI("Begin test Channels");
        //TestChannels();

        logger.LogI("End test");
Disconnect();
    }

    private void TestChannels()
    {

    }

}