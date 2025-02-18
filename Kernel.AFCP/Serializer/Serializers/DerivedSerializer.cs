using System.Reflection;
using System.Reflection.Emit;

public class DerivedSerializer
{
    public static MethodInfo GetDerivedSerialiazer(Serializer ser, KAType TopType)
    {
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Serialize for dervied type: {TopType.FullName}",
            typeof(void),
            [typeof(Serializer), typeof(Stream), TopType.DType]);
        var il = seriliazeMethod.GetILGenerator();

        MethodInfo? writeStream = typeof(Stream).GetMethod("WriteByte", [typeof(byte)]);//Para stream.WriteByte(byte)
        MethodInfo? writeLine = typeof(Console).GetMethod("WriteLine", [typeof(object)]);//Para stream.WriteByte(byte)

        MethodInfo? getType = typeof(object).GetMethod("GetType", []);
        MethodInfo? getAssemblyName = typeof(Type).GetProperty("AssemblyQualifiedName")!.GetMethod;

        MethodInfo? serFunc = typeof(Serializer).GetMethod("Serialize", [typeof(Stream), typeof(object), typeof(Type)]);

        var strWriter = StringeSerializer.GetStringSerializer();

        il.DeclareLocal(typeof(Type));


        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);

        il.Emit(OpCodes.Ldarg_2); //Obj
        il.Emit(OpCodes.Call, getType!);//Type
        il.Emit(OpCodes.Stloc_0);

        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Callvirt, getAssemblyName!);//Assembly name
        il.Emit(OpCodes.Call, strWriter);


        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);

        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Call, serFunc!);


        il.Emit(OpCodes.Ret);

        return seriliazeMethod;
    }


    public static MethodInfo GetDerivedDeserialiazer(Serializer ser, KAType TopType)
    {
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Deserialize for derived type: {TopType.FullName}",
            TopType.DType,
            [typeof(Serializer), typeof(Stream)]);


        var il = seriliazeMethod.GetILGenerator();
        MethodInfo? strRead = StringeSerializer.GetStringDeserializer();
        MethodInfo? getType = typeof(Type).GetMethod("GetType", [typeof(string)]);

        MethodInfo? deserFunc = typeof(Serializer).GetMethod("Deserialize", [typeof(Stream), typeof(Type)]);

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);

        il.Emit(OpCodes.Call, strRead!);
        il.Emit(OpCodes.Call, getType!);


        il.Emit(OpCodes.Call, deserFunc!);

        il.Emit(OpCodes.Ret);
        return seriliazeMethod;
    }


}