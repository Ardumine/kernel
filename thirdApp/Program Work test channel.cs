using System.Numerics;
using Ardumine.Modules.Module1;
using Kernel.AFCP;
using Kernel.Modules;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Start!");

        var ConnectedKernels = new List<KernelDescriptor>();

        var channelManager = new ChannelManager() { ConnectedKernels = ConnectedKernels };

        var moduleManager = new ModuleManager(channelManager);

        channelManager.Join("127.0.0.1", 8000);
        Console.WriteLine("Connected!");


        var interfPos = channelManager.GetInterfaceForChannel<Vector3>("/slamPos");
        var interfMap = channelManager.GetInterfaceForChannel<byte[]>("/SLAMmap");

        interfPos?.AddEvent((aa) =>
        {
            Console.WriteLine("Pos data rec!!" + aa);
        });
        interfMap?.AddEvent((aa) =>
        {
            Console.WriteLine("Map data rec!!");
        });


        Console.WriteLine("Get: " + interfPos?.Get());
        Console.WriteLine("Execing cool func...");

        var interfaceMod1 = moduleManager.GetConector<IModule1>("/modules/mod1");
        interfaceMod1.CoolFunc();
        Console.WriteLine("Done!!!");

        Thread.Sleep(50000);

        channelManager.Stop();

    }
}