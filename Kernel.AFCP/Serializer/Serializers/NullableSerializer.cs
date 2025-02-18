using System.Reflection;
using System.Reflection.Emit;

public class NullableSerializer
{
    public static MethodInfo GetNullSerialiazer(KAType TopType, MethodInfo serMethod)
    {
        //Write 1 se null write 0 se valor.
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Serialize for null type: {TopType.FullName}",
            typeof(void),
            [typeof(Serializer), typeof(Stream), TopType.DType]);
        var il = seriliazeMethod.GetILGenerator();

        MethodInfo? writeStream = typeof(Stream).GetMethod("WriteByte", [typeof(byte)]);//Para stream.WriteByte(byte)

        il.DeclareLocal(typeof(byte));
        Label retLabel = il.DefineLabel();


        il.Emit(OpCodes.Ldarg_2); //Obj
        il.Emit(OpCodes.Ldnull); //null
        il.Emit(OpCodes.Ceq);//Igual? 0/1 
        il.Emit(OpCodes.Stloc_0);//Save loc_0

        //stream.WriteByte(loc_0);
        il.Emit(OpCodes.Ldarg_1);//stream
        il.Emit(OpCodes.Ldloc_0);//ceq res
        il.Emit(OpCodes.Callvirt, writeStream!);//Stream[arg0].WriteByte(loc_0)

        //Há valor? Se 1 então jump para ret(valor é null por isso n precisa de ser). Se 0 fazer continuar para fazer ser
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Brtrue_S, retLabel);

        il.Emit(OpCodes.Ldarg_0); //Load ser
        il.Emit(OpCodes.Ldarg_1); //Load stream
        il.Emit(OpCodes.Ldarg_2); //Load value

        //Ser(Serializer, Stream, Value)
        il.Emit(OpCodes.Call, serMethod);

        il.MarkLabel(retLabel);
        il.Emit(OpCodes.Ret);

        return seriliazeMethod;
    }


    public static MethodInfo GetNullDeserialiazer(KAType TopType, MethodInfo derserMethod)
    {
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Deserialize for null type: {TopType.FullName}",
            TopType.DType,
            [typeof(Serializer), typeof(Stream)]);


        var il = seriliazeMethod.GetILGenerator();

        MethodInfo? readStream = typeof(Stream).GetMethod("ReadByte");//Para stream.ReadByte() -> int

        Label falseLabel = il.DefineLabel();
        Label retLabel = il.DefineLabel();


        il.Emit(OpCodes.Ldarg_1); //Load stream
        il.Emit(OpCodes.Callvirt, readStream!);//stream[arg0].ReadByte()

        //il.Emit(OpCodes.Conv_I1);
        //Há valor? Se 1 então continuar(valor é null). Se 0 fazer jump para falseLabel(tem valor) para fazer deser
        il.Emit(OpCodes.Brfalse_S, falseLabel);

        //É nulo
        il.Emit(OpCodes.Ldnull);
        il.Emit(OpCodes.Br_S, retLabel); //Jump return com valor de null

        il.MarkLabel(falseLabel);//Label tem valor



        il.Emit(OpCodes.Ldarg_0); //Load ser
        il.Emit(OpCodes.Ldarg_1); //Load stream

        //Deser(Serializer, Stream)
        il.Emit(OpCodes.Call, derserMethod);


        il.MarkLabel(retLabel);
        il.Emit(OpCodes.Ret);
        return seriliazeMethod;
    }


}