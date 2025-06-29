using System.Numerics;
using Ardumine.Modules.Module1;
using Kernel.AFCP;
using Kernel.AFCP.KernelManagement;
using Kernel.Logging;
using Kernel.Modules;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Start!");
        var logger = new ConsoleLogger("Main");

        var localKernel = LocalRunningKernel.CreateLocalKernelDescriptor("6c9a24e1-cf5b-40cd-bd05-f451ab539c63");

        using var channelManager = new ChannelManager(localKernel, logger.CreateSubLogger("ChannelManager"));
        var dataChannelManager = new DataChannelManager(channelManager, localKernel, logger.CreateSubLogger("DataChannelManager"));

        var KernelManager = new KernelManager(localKernel, channelManager, dataChannelManager, logger.CreateSubLogger("KernelManager"));


        var moduleManager = new ModuleManager(channelManager, dataChannelManager, localKernel, logger.CreateSubLogger("ModuleManager"));

        Console.WriteLine("Connecting...");
        KernelManager.Join("127.0.0.1", 8000);

        Console.WriteLine("Connected!");


        Console.WriteLine("Execing cool func...");

        var interfaceMod1 = moduleManager.GetModuleInterface<IModule1>("/modules/mod1");
        interfaceMod1.CoolFunc();
        Console.WriteLine("Done!!!");

        Thread.Sleep(5000);
        Console.WriteLine("Exit...");

        KernelManager.Stop();

        Console.WriteLine("Exit ok!");
    }
}