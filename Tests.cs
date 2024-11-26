using Ardumine.Module.Lidar.YDLidar;
using System;
using System.Reflection;
using System.Reflection.Emit;

public class DynamicTypeBuilder
{
    public static Type CreateTypeWithInterface()
    {
        var baseType1 = typeof(YDLidarInterface);
        var baseType2 = typeof(IModuleInterface);

        var typeBuilder = GetTypeBuilder();
        typeBuilder.AddInterfaceImplementation(baseType1);
        typeBuilder.AddInterfaceImplementation(baseType2);

        foreach (var method in baseType1.GetMethods())
        {
            Console.WriteLine($"A fazer: {method.Name} {method.ReturnType.Name}");
            var funcParams = (from b in method.GetParameters() select b.ParameterType).ToList();
            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                method.ReturnType,
                funcParams.ToArray()
            );

            var ilGenerator = methodBuilder.GetILGenerator();
            funcParams.Clear();

            //funcParams.Insert(0, typeof(string));
            //  funcParams.Insert(0, typeof(string));
            funcParams.Add(typeof(object[]));

            if (method.ReturnType == typeof(void))
            {
                var met = typeof(ModuleHelper).GetMethod("RunNormal", funcParams.ToArray());
                //met.Invoke(null, ["/lidar", "SetMotorSpeed", new object[] { 1 }]);
                //ilGenerator.Emit(OpCodes.Ldstr, "/lidar");
                //ilGenerator.Emit(OpCodes.Ldstr, method.Name);
                ilGenerator.Emit(OpCodes.Ldarg_1);

                //ilGenerator.Emit(OpCodes.Newarr, typeof(object));

                ilGenerator.Emit(OpCodes.Call, met);
                ilGenerator.Emit(OpCodes.Ret);
            }
            else
            {
                var met = typeof(ModuleHelper).GetMethod("RunReturn", funcParams.ToArray());
                //met.Invoke(null, ["a", "a"]);
                ilGenerator.Emit(OpCodes.Ldstr, "/lidar");
                ilGenerator.Emit(OpCodes.Ldstr, method.Name);
                ilGenerator.Emit(OpCodes.Call, met);
                ilGenerator.Emit(OpCodes.Ret);
            }


            //ilGenerator.Emit(OpCodes.Call, typeof(CoolClock).GetMethod("GetTime"));

            // ilGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));


            typeBuilder.DefineMethodOverride(methodBuilder, method);
        }

        foreach (var method in baseType2.GetMethods())
        {
            Console.WriteLine($"A fazer: {method.Name}");
            var funcParams = (from b in method.GetParameters() select b.ParameterType).ToList();
            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                 method.ReturnType,
                funcParams.ToArray()
            );

            var ilGenerator = methodBuilder.GetILGenerator();
            funcParams.Insert(0, typeof(string));
            //ilGenerator.Emit(OpCodes.Ldstr, method.Name);
            // ilGenerator.Emit(OpCodes.Call, typeof(ModuleHelper).GetMethod("Run", new Type[] { }));//, typeof(string)
            // ilGenerator.Emit(OpCodes.Ret);
            ilGenerator.Emit(OpCodes.Ldstr, "Hello from the dynamically implemented method!");
            ilGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));

            ilGenerator.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(methodBuilder, baseType2.GetMethod(method.Name));
        }

        return typeBuilder.CreateType();
    }

    private static TypeBuilder GetTypeBuilder()
    {
        var assemblyName = new AssemblyName("DynamicAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        var typeBuilder = moduleBuilder.DefineType(
            "DynamicType",
            TypeAttributes.Public | TypeAttributes.Class
        );

        return typeBuilder;
    }
}





public static class CoolClock
{
    public static DateTime GetTime()
    {
        return DateTime.Now;
    }
}

public interface IMyInterface
{
    DateTime MyMethod();
}

public class DynamicTypeBuilder3
{
    public static Type CreateTypeWithInterface()
    {
        var typeBuilder = GetTypeBuilder();
        typeBuilder.AddInterfaceImplementation(typeof(IMyInterface));

        var methodBuilder = typeBuilder.DefineMethod(
            "MyMethod",
            MethodAttributes.Public | MethodAttributes.Virtual,
            typeof(DateTime),
            Type.EmptyTypes
        );

        var ilGenerator = methodBuilder.GetILGenerator();

        // Call the static GetTime() method on CoolClock
        ilGenerator.Emit(OpCodes.Call, typeof(CoolClock).GetMethod("GetTime"));

        // Return the DateTime value
        ilGenerator.Emit(OpCodes.Ret);

        typeBuilder.DefineMethodOverride(methodBuilder, typeof(IMyInterface).GetMethod("MyMethod"));

        return typeBuilder.CreateType();
    }

    private static TypeBuilder GetTypeBuilder()
    {
        var assemblyName = new AssemblyName("DynamicAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        var typeBuilder = moduleBuilder.DefineType(
            "DynamicType",
            TypeAttributes.Public | TypeAttributes.Class
        );

        return typeBuilder;
    }
}

class Tests
{
    static Logger logger;
    public static void InitTests()
    {
        logger = new Logger("Tests");
    }

    public static void Test2()
    {
        var dynamicType = DynamicTypeBuilder.CreateTypeWithInterface();
        var instance = Activator.CreateInstance(dynamicType);//as IMyInterface

        var dynamicType3 = DynamicTypeBuilder3.CreateTypeWithInterface();
        var instance3 = Activator.CreateInstance(dynamicType3) as IMyInterface;
        var result = instance3.MyMethod();
        Console.WriteLine(result);
        //instance.MyMethod();
    }

    public static void Test1()
    {
        logger.LogOK("Test begin");

        var lidar = ModuleHelper.GetConector<YDLidarInterface>("/lidar");
        // var lidar2 = ModuleHelper.GetConector<YDLidarInterface>("/lidar2");

        lidar.SetMotorSpeed([0, 0]);//Good reference57
                                    //logger.LogI($"Motor speed: {lidar.MotorSpeed}");

        //lidar2.SetMotorSpeed();//80
        // logger.LogI($"Motor speed: {lidar2.MotorSpeed}");


        //var lidarData = lidar.Read();
        //logger.LogI($"Read from the lidar {lidarData.Count} points");
    }
}