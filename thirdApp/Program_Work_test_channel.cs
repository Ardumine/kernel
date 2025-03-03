using System.Numerics;
using Ardumine.Modules.Module1;
using Kernel.AFCP;
using Kernel.Modules;


internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Start!");

        /*      
        class ClassTest
        {
            public Guid guid { get; set; }
        }
        var guid = Guid.NewGuid();
                Console.WriteLine(guid);
                var test = new ClassTest()
                {
                    guid = guid
                };
                var ser = new Serializer();
                var ms = new MemoryStream();

                ser.Serialize(ms, test);
                ms.Position = 0;
                var deser = ser.Deserialize<ClassTest>(ms);
                Console.WriteLine(deser.guid);*/


        var LocalKernel = KernelDescriptor.CreateLocalKernelDescriptor("6c9a24e1-cf5b-40cd-bd05-f451ab539c63");

        var channelManager = new ChannelManager(LocalKernel);

        var KernelManager = new KernelManager()
        {
            LocalKernel = LocalKernel,
            LocalChannelManager = channelManager
        };


        var moduleManager = new ModuleManager(channelManager, KernelManager);

        Console.WriteLine("Connecting...");
        KernelManager.Join("127.0.0.1", 8000);

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

        Thread.Sleep(3000);
        Console.WriteLine("Exit...");

        channelManager.Stop();
        
        

        Console.WriteLine("Exit ok!");
    }
}