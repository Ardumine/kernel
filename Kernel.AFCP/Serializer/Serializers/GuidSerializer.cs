using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

public class GuidSerializer
{

    public static MethodInfo GetGuidSerializer()
    {
        //https://stackoverflow.com/questions/5584160/are-net-enums-blittable-types-marshalling
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Serialize for guid",
            typeof(void),
            [typeof(Serializer), typeof(Stream), typeof(Guid)]);
        var il = seriliazeMethod.GetILGenerator();

        MethodInfo? writeStream = typeof(Stream).GetMethod("Write", [typeof(byte[]), typeof(Int32), typeof(Int32)]);//Para stream.Write(ReadOnlySpan<byte>)
        MethodInfo? toBytes = typeof(Guid).GetMethod("ToByteArray", []);
        MethodInfo? writeLine = typeof(Console).GetMethod("WriteLine", [typeof(object)]);


        il.Emit(OpCodes.Ldarg_1);

        il.Emit(OpCodes.Ldarga, 2);

        il.Emit(OpCodes.Call, toBytes!);


        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldc_I4_S, 16);

        //stream[arg1].Write(Span[value ant])
        il.Emit(OpCodes.Callvirt, writeStream!);//escrever na stream a saida do anterior


        il.Emit(OpCodes.Ret);
        return seriliazeMethod;
    }

    public static MethodInfo GetGuidDeserializer()
    {
        DynamicMethod deseriliazeMethod = new DynamicMethod(
            $"Deserialize for GUID",
            typeof(Guid),
            [typeof(Serializer), typeof(Stream)]);


        var il = deseriliazeMethod.GetILGenerator();
        il.Emit(OpCodes.Nop);

        MethodInfo? streamReadExactly = typeof(Stream).GetMethod("ReadExactly", [typeof(byte[]), typeof(int), typeof(int)]);
        ConstructorInfo? contructorGuid = typeof(Guid).GetConstructor([typeof(byte[])]);

        //byte array
        il.DeclareLocal(typeof(byte[]));
        il.Emit(OpCodes.Nop);


        // Create byte array
        il.Emit(OpCodes.Ldc_I4, 16); //Fazer push da length
        il.Emit(OpCodes.Newarr, typeof(byte)); //Criar byte array
        il.Emit(OpCodes.Stloc_0); //Guardar a byte array em loc_0

        il.Emit(OpCodes.Ldarg_1); //Load stream

        il.Emit(OpCodes.Ldloc_0); //Load byte array
        il.Emit(OpCodes.Ldc_I4_0); //0
        il.Emit(OpCodes.Ldc_I4_S, 16); //objsize


        //Ler os dados da stream
        //stream.ReadExactly(bytes, 0, objsize);
        il.Emit(OpCodes.Callvirt, streamReadExactly!);

        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Newobj, contructorGuid!);

        il.Emit(OpCodes.Ret);
        return deseriliazeMethod;
    }






}