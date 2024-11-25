using System.Net;
using System.Text;
using Ardumine.AFCP.Core.Client;
using Ardumine.AFCP.Core.Client.RawComProt;
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
        var AFCPClient = new AFCPTCPClient(IPAddress.Loopback, true);
        bool stat = AFCPClient.Connect();

        Thread.Sleep(1000);

        if(!stat){
            logger.LogW("Client: wrong password!");
            return;
        }

        logger.LogI("Begin test AFCP client");

        logger.LogI("Sending: Haro? Hibachi, Benihana, Teriyaki...");
        AFCPClient.rawComProt.SendData(60, Encoding.UTF8.GetBytes("Haro? Hibachi, Benihana, Teriyaki..."));

        var dataRead = AFCPClient.rawComProt.ReadData().Data;
        logger.LogI($"Received {Encoding.UTF8.GetString(dataRead)}");//Nagasaki, Okinawa, Hokkaido...Yokohama
        AFCPClient.Close();
    }

}