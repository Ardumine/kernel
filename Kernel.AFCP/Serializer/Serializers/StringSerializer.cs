using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

public class StringeSerializer
{


    public static MethodInfo GetStringSerializer()
    {
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Serializer for string",
            typeof(void),
            [typeof(Serializer), typeof(Stream), typeof(string)]);
        var il = seriliazeMethod.GetILGenerator();

        ConstructorInfo? spanCtor = typeof(ReadOnlySpan<byte>).GetConstructor([typeof(void*), typeof(int)]);//Para criar span de apontador con length

        MethodInfo? stringRef = typeof(System.String).GetMethod("GetPinnableReference", []);//Para stream.Write(ReadOnlySpan<byte>)
        MethodInfo? writeStream = typeof(Stream).GetMethod("Write", [typeof(ReadOnlySpan<byte>)]);//Para stream.Write(ReadOnlySpan<byte>)

        il.DeclareLocal(typeof(int));



        //Para fazer write da len como uint
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Ldc_I4_2);

        //After multipling I need to get a pointer
        il.Emit(OpCodes.Mul);

        il.Emit(OpCodes.Stloc_0);

        il.Emit(OpCodes.Ldarg_1);

        il.Emit(OpCodes.Ldloca, 0);
        il.Emit(OpCodes.Ldc_I4_4);//set ReadOnlySpan length

        il.Emit(OpCodes.Newobj, spanCtor!);//Criar span 


        //stream[arg1].Write(Span[value ant])
        il.Emit(OpCodes.Callvirt, writeStream!);//escrever na stream a saida do anterior





        //para fazer write dos dados em si
        il.Emit(OpCodes.Ldarg_1);

        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Call, stringRef!);

        il.Emit(OpCodes.Ldloc_0);//set ReadOnlySpan length

        il.Emit(OpCodes.Newobj, spanCtor!);//Criar span 

        il.Emit(OpCodes.Callvirt, writeStream!);//escrever na stream a saida do anterior


        il.Emit(OpCodes.Ret);



        return seriliazeMethod;
    }


    public static MethodInfo GetStringDeserializer()
    {
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Deserialzier for string",
            typeof(string),
            [typeof(Serializer), typeof(Stream)]);
        var il = seriliazeMethod.GetILGenerator();


        MethodInfo? streamReadExactly = typeof(Stream).GetMethod("ReadExactly", [typeof(byte[]), typeof(int), typeof(int)]);

        ConstructorInfo? strConstructor = typeof(string).GetConstructor([typeof(char*), typeof(Int32), typeof(Int32)]);

        //len array
        il.DeclareLocal(typeof(byte[]));

        //len
        il.DeclareLocal(typeof(int));

        //data array
        il.DeclareLocal(typeof(byte[]));



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
        il.Emit(OpCodes.Stloc_1);//Save len as uint at loc_1



        /*******PARA LER DADOS*******/

        //Read data
        
        il.Emit(OpCodes.Ldloc_1); //Load len
        il.Emit(OpCodes.Newarr, typeof(byte)); //Criar byte array para dados
        il.Emit(OpCodes.Stloc_2);//Save byte array para loc_2


        il.Emit(OpCodes.Ldarg_1); //Load stream

        il.Emit(OpCodes.Ldloc_2); //Load byte array data
        il.Emit(OpCodes.Ldc_I4_0); //0
        il.Emit(OpCodes.Ldloc_1); //Load len (uint)

        //Ler os dados da stream
        //stream.ReadExactly(bytes, 0, objsize);
        il.Emit(OpCodes.Callvirt, streamReadExactly!);


        /*******PARA CRIAR STRING*******/
        //Obter pointer
        il.Emit(OpCodes.Ldloc_2); //Load byte array data
        il.Emit(OpCodes.Ldc_I4_0);//Obter item 0
        il.Emit(OpCodes.Ldelema, typeof(byte));//Obter endreço
        il.Emit(OpCodes.Conv_U);//??? 

        //Criar string
        il.Emit(OpCodes.Ldc_I4_0);//0
        il.Emit(OpCodes.Ldloc_1);//Load len (uint)
        il.Emit(OpCodes.Ldc_I4_2);
        il.Emit(OpCodes.Div);
        //new string(pointer, 0, len)
        il.Emit(OpCodes.Newobj, strConstructor!); //Criar string

        il.Emit(OpCodes.Ret);
        return seriliazeMethod;
    }

    //https://code-maze.com/csharp-how-to-convert-string-to-span/
    public unsafe static void Serialize(int ser, Stream stream, string val)
    {
        stream.Write(MemoryMarshal.AsBytes(val.AsSpan()));

        fixed (void* p = val)
        {
            stream.Write(new ReadOnlySpan<byte>(p, val.Length * 2));
        }
    }
    public unsafe static void Deserialize(int ser, Stream stream)
    {
        byte[] bytes = new byte[4];
        //stream.ReadExactly(lenBytes, 0, 4);


        int len = bytes.Length;
        fixed (byte* bptr = bytes)
        {
            char* cptr = (char*)bptr;
            var str = new string(cptr, 0, len / 2);
        }
    }

}