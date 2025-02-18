using System.Reflection;
using System.Reflection.Emit;

public class DictSerializer
{

    //https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/Dictionary.cs#L1213
    public static DynamicMethod CreateSerializer(Serializer ser, KAType DictType, KAType KeyType, KAType ValueType)
    {
        DynamicMethod seriliazeMethod = new DynamicMethod(
        $"Serializer for Dict",
        typeof(void),
        [typeof(Serializer), typeof(Stream), DictType.DType]);

        var SerializerForKey = ser.GetMethodForSerialize(seriliazeMethod, KeyType, DictType);
        var SerializerForValue = ser.GetMethodForSerialize(seriliazeMethod, ValueType, DictType);

        var getEntries = DictType.DType.GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var getCount = DictType.DType.GetField("_count", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var spanCtor = typeof(ReadOnlySpan<byte>).GetConstructor([typeof(void*), typeof(int)]);//Para criar span de apontador con length
        var writeStream = typeof(Stream).GetMethod("Write", [typeof(ReadOnlySpan<byte>)]);//Para stream.Write(ReadOnlySpan<byte>)

        var getKeyEntry = getEntries.FieldType.GetElementType()!.GetField("key")!;
        var getValueEntry = getEntries.FieldType.GetElementType()!.GetField("value")!;




        var il = seriliazeMethod.GetILGenerator();
        il.DeclareLocal(typeof(int));//for loop i
        il.DeclareLocal(typeof(int));//len do dict
        il.DeclareLocal(getEntries.FieldType);//itens do dict Entry[]
        il.DeclareLocal(getEntries.FieldType.GetElementType()!.MakePointerType());//Um item da lista


        Label startLoopLabel = il.DefineLabel();
        Label checkLabel = il.DefineLabel();


        //Salvar a len no 1
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldfld, getCount);
        il.Emit(OpCodes.Stloc_1);

        //Salvar a lista de Entry em 2
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldfld, getEntries);
        il.Emit(OpCodes.Stloc_2);

        il.Emit(OpCodes.Ldarg_1);//Stream
        il.Emit(OpCodes.Ldloca, 1);//Pointer to len
        il.Emit(OpCodes.Ldc_I4_4);//uint length(4 bytes)

        il.Emit(OpCodes.Newobj, spanCtor!);//Criar span do pointer da len

        //stream[arg1].Write(Span[value ant])
        il.Emit(OpCodes.Callvirt, writeStream!);//escrever na stream a saida do anterior


        //i = 0
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc_0);
        //jump para check
        il.Emit(OpCodes.Br_S, checkLabel);
        il.MarkLabel(startLoopLabel);
        //codigo dentro do for
        //{
        {


            il.Emit(OpCodes.Ldloc_2);//arr dict
            il.Emit(OpCodes.Ldloc_0);//i
            il.Emit(OpCodes.Ldelema, getEntries.FieldType.GetElementType()!);//Load elem as &
            il.Emit(OpCodes.Stloc_3);
            //il.Emit(OpCodes.Dup);

            il.Emit(OpCodes.Ldarg_0);//Load arg Serializer
            il.Emit(OpCodes.Ldarg_1);//Load stream
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Ldfld, getKeyEntry);
            il.Emit(OpCodes.Call, SerializerForKey);


            il.Emit(OpCodes.Ldarg_0);//Load arg Serializer
            il.Emit(OpCodes.Ldarg_1);//Load stream
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Ldfld, getValueEntry);
            il.Emit(OpCodes.Call, SerializerForValue);

            //il.Emit(OpCodes.Ldfld, getValueEntry);
            // il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", [typeof(uint)])!);
        }
        //}
        //i++;
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc_0);


        //i<len
        il.MarkLabel(checkLabel);
        il.Emit(OpCodes.Ldloc_0);//i
        il.Emit(OpCodes.Ldloc_1);//len do dict
        il.Emit(OpCodes.Clt);//comp
        il.Emit(OpCodes.Brtrue_S, startLoopLabel);



        il.Emit(OpCodes.Ret);
        return seriliazeMethod;

    }


    public static DynamicMethod CreateDeserializer(Serializer ser, KAType DictType, KAType KeyType, KAType ValueType)
    {
        DynamicMethod deserializeMethod = new DynamicMethod(
        $"Deserializer for Dict",
        DictType.DType,
        [typeof(Serializer), typeof(Stream)]);

        var DeserializerForKey = ser.GetMethodForDeserialize(deserializeMethod, KeyType, DictType);
        var DeserializerForValue = ser.GetMethodForDeserialize(deserializeMethod, ValueType, DictType);

        MethodInfo? streamReadExactly = typeof(Stream).GetMethod("ReadExactly", [typeof(byte[]), typeof(int), typeof(int)]);
        MethodInfo? tryAddDict = DictType.DType.GetMethod("TryAdd", [KeyType.DType, ValueType.DType]);

        ConstructorInfo? dictConstructor = DictType.DType.GetConstructor([typeof(int)]);



        var il = deserializeMethod.GetILGenerator();
        il.DeclareLocal(typeof(int));//for loop i
        il.DeclareLocal(typeof(int));//len do dict
        il.DeclareLocal(typeof(byte[]));//read data
        il.DeclareLocal(DictType.DType);



        Label startLoopLabel = il.DefineLabel();
        Label checkLabel = il.DefineLabel();

        //Read Len
        {
            il.Emit(OpCodes.Ldc_I4_4); //Fazer push da length(uint = 4 bytes)
            il.Emit(OpCodes.Newarr, typeof(byte)); //Criar byte array
            il.Emit(OpCodes.Stloc_2); //Guardar a byte array em loc_0

            //read
            il.Emit(OpCodes.Ldarg_1); //Load stream

            il.Emit(OpCodes.Ldloc_2); //Load byte array len
            il.Emit(OpCodes.Ldc_I4_0); //0
            il.Emit(OpCodes.Ldc_I4_4); //uint len = 4 bytes


            //Ler os dados da stream
            //stream.ReadExactly(bytes, 0, objsize);
            il.Emit(OpCodes.Callvirt, streamReadExactly!);
            il.Emit(OpCodes.Nop);



            //Obter pointer para length
            il.Emit(OpCodes.Ldloc_2); //Load byte array len
            il.Emit(OpCodes.Ldc_I4_0);//Obter item 0

            il.Emit(OpCodes.Ldelema, typeof(byte));//Obter endreço
            il.Emit(OpCodes.Conv_U);//??? 


            // Obter uint
            il.Emit(OpCodes.Ldobj, typeof(uint));
            il.Emit(OpCodes.Stloc_1);//Save len as uint at loc_1

        }

        //Create dict obj
        {
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Newobj, dictConstructor!);
            il.Emit(OpCodes.Stloc_3);
        }

        //For loop
        {
            //i = 0
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_0);
            //jump para check
            il.Emit(OpCodes.Br_S, checkLabel);
            il.MarkLabel(startLoopLabel);
            //codigo dentro do for
            {
                il.Emit(OpCodes.Ldloc_3);

                il.Emit(OpCodes.Ldarg_0);//Arg do serializer
                il.Emit(OpCodes.Ldarg_1);//Arg do stream
                il.Emit(OpCodes.Call, DeserializerForKey);

                il.Emit(OpCodes.Ldarg_0);//Arg do serializer
                il.Emit(OpCodes.Ldarg_1);//Arg do stream
                il.Emit(OpCodes.Call, DeserializerForValue);

                //dict[l3].TryAdd(key, val); -> ret bool(faz pop)
                il.Emit(OpCodes.Callvirt, tryAddDict!);
                il.Emit(OpCodes.Pop);
            }


            //i++;
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_0);


            //i<len
            il.MarkLabel(checkLabel);
            il.Emit(OpCodes.Ldloc_0);//i
            il.Emit(OpCodes.Ldloc_1);//len do dict
            il.Emit(OpCodes.Clt);//comp
            il.Emit(OpCodes.Brtrue_S, startLoopLabel);
        }

        il.Emit(OpCodes.Ldloc_3);
        il.Emit(OpCodes.Ret);
        return deserializeMethod;
    }



    public static void Deserialize(Serializer ser, Stream stream)
    {

    }
}