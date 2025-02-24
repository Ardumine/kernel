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

        var moduleManager = new ModuleManager(channelManager, KernelDescriptor.CreateLocalKernelDescriptor());

        channelManager.Join("127.0.0.1", 8000);
        Console.WriteLine("Connected!");


        var interfPos = channelManager.GetDataChannelInterface<Vector3>("/slamPos");
        var interfMap = channelManager.GetDataChannelInterface<byte[]>("/SLAMmap");

        interfPos?.AddEvent((pos) =>
        {
            Console.WriteLine("Pos data rec!!" + pos);
        });
        interfMap?.AddEvent((aa) =>
        {
            Console.WriteLine("Map data rec!!");
        });


        Console.WriteLine("Get: " + interfPos?.Get());
        Console.WriteLine("Execing cool func...");

        var interfaceMod1 = moduleManager.GetModuleInterface<IModule1>("/modules/mod1");
        interfaceMod1.CoolFunc();
        Console.WriteLine("Done!!!");

        Thread.Sleep(50000);

        channelManager.Stop();

    }
}