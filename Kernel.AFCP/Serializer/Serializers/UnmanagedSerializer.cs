using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

public class UnmanagedSerializer
{
    private static int GetObjectSize(Type type)
    {
        int objSize = 0;

        if (type.IsEnum)
        {
            objSize = Marshal.SizeOf(Enum.GetUnderlyingType(type));
        }
        else
        {
            objSize = Marshal.SizeOf(type);
        }

        return objSize;
    }
    public static MethodInfo GetUnamangedStructSerializer(KAType structType)
    {
        //https://stackoverflow.com/questions/5584160/are-net-enums-blittable-types-marshalling
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Serialize for struct type: {structType.FullName}",
            typeof(void),
            [typeof(Serializer), typeof(Stream), structType.DType]);
        var il = seriliazeMethod.GetILGenerator();

        ConstructorInfo? spanCtor = typeof(ReadOnlySpan<byte>).GetConstructor([typeof(void*), typeof(int)]);//Para criar span de apontador con length
        MethodInfo? writeStream = typeof(Stream).GetMethod("Write", [typeof(ReadOnlySpan<byte>)]);//Para stream.Write(ReadOnlySpan<byte>)
        int objSize = GetObjectSize(structType.DType);

        //Criar para stloc_0(return val)
        il.DeclareLocal(structType.DType.MakePointerType());

        il.Emit(OpCodes.Ldarga_S, 2);//Obter apontador(segundo argumento)
        il.Emit(OpCodes.Conv_U);//???
        il.Emit(OpCodes.Stloc_0);//Guardar apontador

        il.Emit(OpCodes.Ldarg_1);//load stream

        il.Emit(OpCodes.Ldloc_0);//load pointer
        il.Emit(OpCodes.Ldc_I4, objSize);//set ReadOnlySpan length

        il.Emit(OpCodes.Newobj, spanCtor!);//Criar span 

        //stream[arg1].Write(Span[value ant])
        il.Emit(OpCodes.Callvirt, writeStream!);//escrever na stream a saida do anterior


        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Ret);
        return seriliazeMethod;
    }

    public static MethodInfo GetUnamangedStructDeserializer(KAType structType)
    {
        DynamicMethod deseriliazeMethod = new DynamicMethod(
            $"Deserialize for struct type: {structType.FullName}",
            structType.DType,
            [typeof(Serializer), typeof(Stream)]);

        int objSize = GetObjectSize(structType.DType);

        var il = deseriliazeMethod.GetILGenerator();
        il.Emit(OpCodes.Nop);

        MethodInfo? streamReadExactly = typeof(Stream).GetMethod("ReadExactly", [typeof(byte[]), typeof(int), typeof(int)]);

        //byte array
        il.DeclareLocal(typeof(byte[]));

        il.Emit(OpCodes.Nop);


        // Create byte array
        il.Emit(OpCodes.Ldc_I4, objSize); //Fazer push da length
        il.Emit(OpCodes.Newarr, typeof(byte)); //Criar byte array
        il.Emit(OpCodes.Stloc_0); //Guardar a byte array em loc_0

        il.Emit(OpCodes.Ldarg_1); //Load stream

        il.Emit(OpCodes.Ldloc_0); //Load byte array
        il.Emit(OpCodes.Ldc_I4_0); //0
        il.Emit(OpCodes.Ldc_I4, objSize); //objsize


        //Ler os dados da stream
        //stream.ReadExactly(bytes, 0, objsize);
        il.Emit(OpCodes.Callvirt, streamReadExactly!);
        il.Emit(OpCodes.Nop);



        il.Emit(OpCodes.Ldloc_0); //Load byte array
        il.Emit(OpCodes.Ldc_I4_0);//Obter item 0

        il.Emit(OpCodes.Ldelema, typeof(byte));//Obter endreço
        il.Emit(OpCodes.Conv_U);//??? 


        // Converter obj para pointer
        il.Emit(OpCodes.Ldobj, structType.DType);
        il.Emit(OpCodes.Nop);

        il.Emit(OpCodes.Ret);
        return deseriliazeMethod;
    }





}