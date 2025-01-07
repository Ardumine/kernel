using System.Numerics;
using AFCP;
using Kernel.Modules;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Start!");

        var ConnectedKernels = new List<KernelDescriptor>();

        var channelManager = new ChannelManager() { ConnectedKernels = ConnectedKernels };

        //var moduleManager = new ModuleManager(channelManager);

        channelManager.Join("127.0.0.1", 8000);

        //var lidarDataChannel = channelManager.GetDataChannel("/slamPos");
        var interf = channelManager.GetInterfaceForChannel<Vector3>("/slamPos");
        interf?.AddEvent((aa) =>
        {
            Console.WriteLine("Lidar data rec!!" + aa);
        });
        Console.WriteLine("Get: " + interf?.Get());

        Thread.Sleep(50000);

        channelManager.Stop();

    }
}