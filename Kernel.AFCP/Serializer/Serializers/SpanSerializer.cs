using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

public class SpanSerializer
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

    public static MethodInfo GetUnmanagedSpanSerializer(KAType type)
    {
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Serialize for span unamanaged type: {type.FullName}",
            typeof(void),
            [typeof(Serializer), typeof(Stream), type.DType]);
        var il = seriliazeMethod.GetILGenerator();

        var SpanType = type.DType;
        var itemType = type.SubHoldingType[0]!.DType;



        MethodInfo asBytes = typeof(MemoryMarshal).GetMethods().Where(p => p.Name == "AsBytes").First().MakeGenericMethod(itemType);

        ConstructorInfo? spanCtor = typeof(ReadOnlySpan<byte>).GetConstructor([typeof(void*), typeof(int)]);//Para criar span de apontador con length
        MethodInfo? writeStream = typeof(Stream).GetMethod("Write", [typeof(ReadOnlySpan<byte>)]);//Para stream.Write(ReadOnlySpan<byte>)

        il.DeclareLocal(typeof(int));



        //Obter len em pointer
        il.Emit(OpCodes.Ldarga, 2); //Obter apontador dados
        il.Emit(OpCodes.Ldlen); //Obter len
        il.Emit(OpCodes.Stloc_0);//Save len


        il.Emit(OpCodes.Ldarg_1);//Stream
        il.Emit(OpCodes.Ldloca, 0);//Pointer to len
        il.Emit(OpCodes.Ldc_I4_4);//uint length(4 bytes)

        il.Emit(OpCodes.Newobj, spanCtor!);//Criar span do pointer da len

        //stream[arg1].Write(Span[value ant])
        il.Emit(OpCodes.Callvirt, writeStream!);//escrever na stream a saida do anterior


        //Enviar dados
        il.Emit(OpCodes.Ldarg_1); //Load stream
        il.Emit(OpCodes.Ldarg_2); //Load data
        il.Emit(OpCodes.Call, asBytes); //Transform data to Span<byte>
        il.Emit(OpCodes.Callvirt, writeStream!);//Send


        il.Emit(OpCodes.Ret);
        return seriliazeMethod;
    }

    public static MethodInfo GetUnmanagedSpanDeserializer(KAType type)
    {
        var SpanType = type.DType;
        var itemType = type.SubHoldingType[0]!.DType;
        int ItemSize = GetObjectSize(itemType);

        DynamicMethod deseriliazeMethod = new DynamicMethod(
           $"Deserialize for span unamanaged type: {type.FullName}",
           SpanType,
            [typeof(Serializer), typeof(Stream)]);

        var il = deseriliazeMethod.GetILGenerator();

        ConstructorInfo byteSpanCtor = typeof(Span<byte>).GetConstructor([typeof(byte[])])!;
        MethodInfo Cast = typeof(MemoryMarshal).GetMethods().Where(p => p.Name == "Cast").First().MakeGenericMethod(typeof(byte), itemType);
        MethodInfo? streamReadExactly = typeof(Stream).GetMethod("ReadExactly", [typeof(byte[]), typeof(int), typeof(int)]);



        il.DeclareLocal(typeof(byte[]));
        il.DeclareLocal(typeof(int));




        /*******PARA LER LEN*******/

        // Create byte array len
        il.Emit(OpCodes.Ldc_I4_4); //Fazer push da length(uint = 4 bytes)
        il.Emit(OpCodes.Newarr, typeof(byte)); //Criar byte array
        il.Emit(OpCodes.Stloc_0); //Guardar a byte array em loc_0

        //read
        il.Emit(OpCodes.Ldarg_1); //Load stream

        il.Emit(OpCodes.Ldloc_0); //Load byte array len
        il.Emit(OpCodes.Ldc_I4_0); //0
        il.Emit(OpCodes.Ldc_I4_4); //uint len = 4 bytes


        //Ler os dados da stream
        //stream.ReadExactly(bytes, 0, objsize);
        il.Emit(OpCodes.Callvirt, streamReadExactly!);
        il.Emit(OpCodes.Nop);



        //Obter pointer para length
        il.Emit(OpCodes.Ldloc_0); //Load byte array len
        il.Emit(OpCodes.Ldc_I4_0);//Obter item 0

        il.Emit(OpCodes.Ldelema, typeof(byte));//Obter endreço
        il.Emit(OpCodes.Conv_U);//??? 


        // Obter uint
        il.Emit(OpCodes.Ldobj, typeof(uint));
        il.Emit(OpCodes.Ldc_I4, ItemSize);
        il.Emit(OpCodes.Mul);

        il.Emit(OpCodes.Stloc_1);//Save len as uint at loc_1


        //**READ DATA**//
        il.Emit(OpCodes.Ldloc_1); //Load len
        il.Emit(OpCodes.Newarr, typeof(byte)); //Criar byte array para dados
        il.Emit(OpCodes.Stloc_0);//Save byte array para loc_1


        il.Emit(OpCodes.Ldarg_1); //Load stream

        il.Emit(OpCodes.Ldloc_0); //Load byte array data
        il.Emit(OpCodes.Ldc_I4_0); //0
        il.Emit(OpCodes.Ldloc_1); //Load len (uint)

        //Ler os dados da stream
        //stream.ReadExactly(bytes, 0, objsize);
        il.Emit(OpCodes.Callvirt, streamReadExactly!);

        il.Emit(OpCodes.Ldloc_0);//2
        il.Emit(OpCodes.Newobj, byteSpanCtor!);

        //**Convert data**//
        il.Emit(OpCodes.Call, Cast);

        il.Emit(OpCodes.Ret);


        return deseriliazeMethod;
    }



}