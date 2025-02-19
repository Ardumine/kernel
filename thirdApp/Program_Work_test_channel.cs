using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Ardumine.Modules.Module1;
using Kernel.AFCP;
using Kernel.Modules;
using Kernel.Modules.Base;




internal class Program
{
    private static T CreateNewObj<T>() where T : struct
    {
        return new T();
    }
    //https://stackoverflow.com/questions/4989681/place-an-object-on-top-of-stack-in-ilgenerator
    public static void Emit_LdInst<T>(ILGenerator il, T inst) where T : class
    {
        var gch = GCHandle.Alloc(inst);

        var ptr = GCHandle.ToIntPtr(gch);

        if (IntPtr.Size == 4)
            il.Emit(OpCodes.Ldc_I4, ptr.ToInt32());
        else
            il.Emit(OpCodes.Ldc_I8, ptr.ToInt64());

        il.Emit(OpCodes.Ldobj, typeof(T));

        // Do the following only if you can elsewhere ensure that 'inst'
        // outlives this DynamicMethod
        // gch.Free();
    }
    private static void Main(string[] args)
    {
        ModuleManager moduleManager = null!;
        RModule targetRModule = null!;

        // Create a dynamic assembly and module
        AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

        // Define the type that implements the interface
        TypeBuilder typeBuilder = moduleBuilder.DefineType("DerivedClass", TypeAttributes.Public);
        typeBuilder.AddInterfaceImplementation(typeof(IModuleConector)); // Implement the interface

        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                "GetDataChannel",
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis);

            methodBuilder.SetReturnType(typeof(DataChannelDescriptor));
            methodBuilder.SetParameters([typeof(string)]);


            ILGenerator il = methodBuilder.GetILGenerator();

            MethodInfo getDataChannel = typeof(ModuleManager).GetMethod("GetDataChannel", BindingFlags.Public)!;

            Emit_LdInst(il, moduleManager);
            Emit_LdInst(il, targetRModule);
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Call, getDataChannel);
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, typeof(IModuleConector).GetMethod("GetDataChannel")!);
        }

        
        // Create the type
        Type derivedType = typeBuilder.CreateType();

        // Create an instance of the dynamically created type
        object instance = Activator.CreateInstance(derivedType)!;

        // Cast to the interface and call the method
        IModuleConector derivedInstance = (IModuleConector)instance;
        var result = derivedInstance.GetDataChannel("a"); // Example with int
        Console.WriteLine($"Result: {result}");

    }
    private static void Main2(string[] args)
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