using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Kernel.AFCP;

namespace Kernel.Modules.Base;

public interface IModuleConector
{
    public DataChannelDescriptor? GetDataChannel(string Path);
    //public DataChannelDescriptor InitiateChannel<T>(string Path);
    //public DataChannelInterface<T> GetChannelInterface<T>(string Path);
}


public class ModuleConectorHelper{

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


    public static IModuleConector GetModuleConector(ModuleManager moduleManager, RModule targetRModule){

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

            MethodInfo getDataChannel = typeof(ModuleManager).GetMethod("GetDataChannel")!;

            Emit_LdInst(il, moduleManager);
            Emit_LdInst(il, targetRModule);
            il.Emit(OpCodes.Ldarg_1);

            il.Emit(OpCodes.Call, getDataChannel);
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, typeof(IModuleConector).GetMethod("GetDataChannel")!);
        }

        
        Type derivedType = typeBuilder.CreateType();


        return  (IModuleConector)Activator.CreateInstance(derivedType)!;
    }
}