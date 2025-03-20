using System.Net.Sockets;
using Kernel.AFCP.DataTreatment;

namespace Kernel.AFCP;

public class AfcpTCPClient : IDisposable
{
    public bool Running { get; private set; }


    private TcpClient? TcpClient { get; set; }
    private NetworkStream? TCPClientstream;

    private AutoResetEvent OnNewData = new AutoResetEvent(false);
    private Queue<DataWritter> DataCache = new();

    private Thread? handleTCPClientReadThread;
    private Thread? handleTCPClientWriteThread;


    public delegate void OnDataRecEvent(DataReader reader);
    public event OnDataRecEvent? OnDataRec;

    public delegate void OnClientDisconnectEvent(DisconnectReason reason);
    public event OnClientDisconnectEvent? OnClientDisconnect;

    public AfcpTCPClient()
    {

    }

    public AfcpTCPClient(TcpClient client, string IP, int Port)
    {
        TcpClient = client;
        TCPClientstream = TcpClient.GetStream();
        Running = true;


        StartThreads(IP, Port);
    }



    public void WriteData(DataWritter dataWritter)
    {
        DataCache.Enqueue(dataWritter);
        OnNewData.Set();
    }

    public void Connect(string IP, int Port)
    {
        TcpClient = new TcpClient();

        TcpClient.Connect(IP, Port);
        TCPClientstream = TcpClient.GetStream();
        Running = true;



        StartThreads(IP, Port);
    }

    public void StartThreads(string IP, int Port)
    {
        handleTCPClientReadThread = new Thread(handleTCPClientRead)
        {
            Name = $"AfcpClient {IP}:{Port} Read"
        };

        handleTCPClientWriteThread = new Thread(handleTCPClientWrite)
        {
            Name = $"AfcpClient {IP}:{Port} Send"
        };

        handleTCPClientReadThread.Start();
        handleTCPClientWriteThread.Start();
    }

    private void handleTCPClientWrite()
    {
        while (Running)
        {
            while (DataCache.Count > 0)
            {
                if (TCPClientstream == null)
                    break;
                var writter = DataCache.Dequeue();
                writter.Copy(TCPClientstream);

                writter.Dispose();
            }
            OnNewData.Reset();
            OnNewData.WaitOne(new TimeSpan(0, 0, 5));//In case stuck, just reloop in 5 secs
        }
        Console.WriteLine("Exit Write");
    }

    private void handleTCPClientRead()
    {
        while (Running)
        {
            try
            {
                if (TCPClientstream == null)
                {
                    break;
                }
                var reader = new DataReader(TCPClientstream);
                if (OnDataRec != null)
                {
                    OnDataRec.Invoke(reader);
                }
            }
            catch (EndOfStreamException)
            {
                Stop();
            }
            catch
            {
                if (TcpClient != null && TcpClient!.Connected && Running)
                {
                    OnDisconnect(DisconnectReason.ConnectionLost);
                }
                if (Running && TcpClient != null && TcpClient.Connected)
                {
                    throw;
                }


            }

        }
        Console.WriteLine("exit read");

    }
    public void OnDisconnect(DisconnectReason reason)
    {
        if (OnClientDisconnect != null)
        {
            OnClientDisconnect.Invoke(reason);
        }
        Stop();
    }

    private void Stop()
    {
        Running = false;
        TcpClient?.Close();
        TCPClientstream?.Close();
        OnNewData.Set();
        DataCache.Clear();

    }
    public void Dispose()
    {
        Stop();
        TcpClient?.Dispose();
        TCPClientstream?.Dispose();
        OnNewData.Dispose();
        DataCache.Clear();


    }
}

public enum DisconnectReason
{
    RequestedBySelf,
    RequestedByRemote,
    ConnectionLost,
}