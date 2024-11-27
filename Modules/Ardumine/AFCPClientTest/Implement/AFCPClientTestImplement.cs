using System.Net;
using System.Text;
using Ardumine.AFCP.Core.Client;
using Ardumine.Module.Base;

namespace Ardumine.Module.AFCPClientTest;
public class AFCPClientTestImplement : AFCPClientTestInterface, BaseImplement
{
    public required string Path { get; set; }
    public Guid guid { get; set; }
    public required Logger logger { get; set; }

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
    private void Disconnect()
    {
        Thread.Sleep(10);
        AFCPClient.Close();
    }
    private void Main()
    {
        Thread.Sleep(200);//Let the rest of the kernel boot
        logger.Space();

        logger.LogOK("Begin test AFCP client");

        AFCPClient = new AFCPTCPClient(IPAddress.Loopback, true);

        logger.LogI("Conecting...");
        Connect();

        logger.LogI("Begin test Haro....");
        TestHaro();

        logger.LogI("Begin test Channels...");
        TestChannels();

        logger.LogOK("End test...");
        Disconnect();
    }


    private void Connect()
    {

        bool stat = AFCPClient.Connect();

        logger.LogI("Client connected!");
        if (!stat)
        {
            logger.LogW("Client: wrong password!");
            return;
        }
        Thread.Sleep(50);

    }

    public void TestHaro()
    {
        logger.LogI("Sending: Haro? Hibachi, Benihana, Teriyaki...");
        AFCPClient.rawComProt.SendData(280, Encoding.UTF8.GetBytes("Haro? Hibachi, Benihana, Teriyaki..."));

        var dataRead = AFCPClient.ReadChannelData(280);
        logger.LogI($"Received {Encoding.UTF8.GetString(dataRead)}");//Nagasaki, Okinawa, Hokkaido...Yokohama
    }

    private void TestChannels()
    {
        var aa = AFCPClient.Ask(499, []);
        logger.LogI($"Testing channels 1: {string.Join(",", aa)}");

        var data = ChannelManager.ReadData("/oi");
        logger.LogI($"Testing channels 2: {string.Join(",", data)}");

        var channel = ChannelManager.GetChannel("/oi", AFCPClient);
        logger.LogI($"Testing channels 3.1: {channel.AFCP_ID}");
        logger.LogI($"Testing channels 3.2: {string.Join(",", channel.Get())}");
        channel.Set([1, 2, 3]);
        logger.LogI($"Testing channels 3.3: {string.Join(",", channel.Get())}");



    }

}