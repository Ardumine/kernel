using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

public class ArraySerializer
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

    public static MethodInfo GetUnmanagedArraySerializer(KAType type)
    {
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Serialize for array unamanaged type: {type.FullName}",
            typeof(void),
            [typeof(Serializer), typeof(Stream), type.DType]);
        var il = seriliazeMethod.GetILGenerator();

        var itemType = type.SubHoldingType[0]!.DType;


        int objSize = GetObjectSize(itemType);


        ConstructorInfo? spanCtor = typeof(ReadOnlySpan<byte>).GetConstructor([typeof(void*), typeof(int)]);//Para criar span de apontador con length
        MethodInfo? writeStream = typeof(Stream).GetMethod("Write", [typeof(ReadOnlySpan<byte>)]);//Para stream.Write(ReadOnlySpan<byte>)

        il.DeclareLocal(typeof(int));



        //Obter len em pointer
        il.Emit(OpCodes.Ldarg_2); //Obter apontador dados
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

        //Obter apontador array
        il.Emit(OpCodes.Ldarg_2); //Load byte array len
        il.Emit(OpCodes.Ldc_I4_0);//Obter item 0

        il.Emit(OpCodes.Ldelema, typeof(byte));//Obter endreço
        il.Emit(OpCodes.Conv_U);//??? 

        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldlen); //Obter num objs array
        il.Emit(OpCodes.Ldc_I4, objSize);
        il.Emit(OpCodes.Mul);// Num objs * cada obj size


        il.Emit(OpCodes.Newobj, spanCtor!);//Criar span dos dados
        il.Emit(OpCodes.Callvirt, writeStream!);//Send


        il.Emit(OpCodes.Ret);
        return seriliazeMethod;
    }

    public static MethodInfo GetUnmanagedArrayDeserializer(KAType type)
    {
        var arrayType = type.DType;
        var itemType = type.SubHoldingType[0]!.DType;
        int ItemSize = GetObjectSize(itemType);

        DynamicMethod deseriliazeMethod = new DynamicMethod(
           $"Deserialize for array unamanaged type: {type.FullName}",
           arrayType,
           [typeof(Serializer), typeof(Stream)]);

        var il = deseriliazeMethod.GetILGenerator();

        ConstructorInfo byteSpanCtor = typeof(Span<byte>).GetConstructor([typeof(byte[])])!;
        MethodInfo Cast = typeof(MemoryMarshal).GetMethods().Where(p => p.Name == "Cast").First().MakeGenericMethod(typeof(byte), itemType);

        MethodInfo? streamReadExactly = typeof(Stream).GetMethod("ReadExactly", [typeof(byte[]), typeof(int), typeof(int)]);

        MethodInfo? toArray = typeof(ReadOnlySpan<>).MakeGenericType(itemType).GetMethod("ToArray", []);

        il.DeclareLocal(typeof(byte[]));
        il.DeclareLocal(typeof(int));
        il.DeclareLocal(typeof(ReadOnlySpan<>).MakeGenericType(itemType));




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
        il.Emit(OpCodes.Stloc_2);


        //Obter array
        il.Emit(OpCodes.Ldloca_S, 2);//Ns pq mas é preciso ser apontador. Talvez pq a span têm a coisa de ser segura ou algo do genero?
        il.Emit(OpCodes.Call, toArray!);

        il.Emit(OpCodes.Ret);


        return deseriliazeMethod;
    }



    public static MethodInfo GetManagedArraySerializer(Serializer ser, KAType type)
    {
        DynamicMethod serializeMethod = new DynamicMethod(
            $"Serialize for array managed type: {type.FullName}",
            typeof(void),
            [typeof(Serializer), typeof(Stream), type.DType]);
        var il = serializeMethod.GetILGenerator();

        var itemType = type.SubHoldingType[0]!.DType;


        var SerializerForValue = ser.GetMethodForSerialize(serializeMethod, type.SubHoldingType[0], type);


        ConstructorInfo? spanCtor = typeof(ReadOnlySpan<byte>).GetConstructor([typeof(void*), typeof(int)]);//Para criar span de apontador con length
        MethodInfo? writeStream = typeof(Stream).GetMethod("Write", [typeof(ReadOnlySpan<byte>)]);//Para stream.Write(ReadOnlySpan<byte>)

        il.DeclareLocal(typeof(int));//for loop i
        il.DeclareLocal(typeof(int));//len da lista

        Label startLoopLabel = il.DefineLabel();
        Label checkLabel = il.DefineLabel();

        //Obter a len
        il.Emit(OpCodes.Ldarg_2); //Obter apontador dados
        il.Emit(OpCodes.Ldlen); //Obter len
        il.Emit(OpCodes.Stloc_1);//Save len

        //Write Len
        {
            il.Emit(OpCodes.Ldarg_1);//Stream
            il.Emit(OpCodes.Ldloca, 1);//Pointer to len
            il.Emit(OpCodes.Ldc_I4_4);//uint length(4 bytes)

            il.Emit(OpCodes.Newobj, spanCtor!);//Criar span do pointer da len

            //stream[arg1].Write(Span[value ant])
            il.Emit(OpCodes.Callvirt, writeStream!);//escrever na stream a saida do anterior
        }


        //For loop write data
        {
            //i = 0
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_0);
            //jump para check
            il.Emit(OpCodes.Br_S, checkLabel);
            il.MarkLabel(startLoopLabel);
            //codigo dentro do for
            //{
            {
                il.Emit(OpCodes.Ldarg_0);//Load arg Serializer
                il.Emit(OpCodes.Ldarg_1);//Load stream


                il.Emit(OpCodes.Ldarg_2);//arr
                il.Emit(OpCodes.Ldloc_0);//i
                il.Emit(OpCodes.Ldelem, itemType);//Load elem

                il.Emit(OpCodes.Call, SerializerForValue);
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
        }

        il.Emit(OpCodes.Ret);
        return serializeMethod;
    }

    public static MethodInfo GetManagedArrayDeserializer(Serializer ser, KAType type)
    {
        var arrayType = type.DType;
        var itemType = type.SubHoldingType[0]!.DType;

        DynamicMethod deserializeMethod = new DynamicMethod(
           $"Deserialize for array amanaged type: {type.FullName}",
           arrayType,
           [typeof(Serializer), typeof(Stream)]);

        var il = deserializeMethod.GetILGenerator();
        Label startLoopLabel = il.DefineLabel();
        Label checkLabel = il.DefineLabel();


        MethodInfo? streamReadExactly = typeof(Stream).GetMethod("ReadExactly", [typeof(byte[]), typeof(int), typeof(int)]);

        var DeserializerForValue = ser.GetMethodForDeserialize(deserializeMethod, type.SubHoldingType[0]!, type);


        il.DeclareLocal(typeof(int));//for loop i
        il.DeclareLocal(typeof(int));//len da arr
        il.DeclareLocal(typeof(byte[]));//read data

        il.DeclareLocal(arrayType);//array 


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


            //Obter pointer para length
            il.Emit(OpCodes.Ldloc_2); //Load byte array len
            il.Emit(OpCodes.Ldc_I4_0);//Obter item 0

            il.Emit(OpCodes.Ldelema, typeof(byte));//Obter endreço
            il.Emit(OpCodes.Conv_U);//??? 


            // Obter uint
            il.Emit(OpCodes.Ldobj, typeof(uint));
            il.Emit(OpCodes.Stloc_1);//Save len as uint at loc_1

        }


        //Create array obj
        {
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Newarr, itemType);
            il.Emit(OpCodes.Stloc_3);
        }

        //Read data
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
                il.Emit(OpCodes.Ldloc_0);



                il.Emit(OpCodes.Ldarg_0);//Arg do serializer
                il.Emit(OpCodes.Ldarg_1);//Arg do stream
                il.Emit(OpCodes.Call, DeserializerForValue);
                il.Emit(OpCodes.Stelem, itemType);
                //list[l3].Add(val);
                //il.Emit(OpCodes.Callvirt, addList!);
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





}