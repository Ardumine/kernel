using System.Reflection;
using System.Reflection.Emit;
using Kernel.AFCP.KASerializer.Atributes;

public struct KAClassProperty
{
    public required string Name { get; set; }
    public required KAType Type { get; set; }

    public required MethodInfo GetMethod { get; set; }
    public required MethodInfo SetMethod { get; set; }
}

public class ClassSerializer
{

    //https://sharplab.io/
    public static MethodInfo CreateClassSerializerMethod(Serializer ser, KAType type)
    {
        //estático não têm inscancia. Se tivese, a intancia desse mesmo objecto estaria em 0, por isso os "verdadeiros" arugmentos comçam em 1 inclusive.
        DynamicMethod seriliazeMethod = new DynamicMethod(
            $"Serialize for class type: {type.FullName}",
            typeof(void),
            [typeof(Serializer), typeof(Stream), type.DType]);
        var il = seriliazeMethod.GetILGenerator();


        foreach (var prop in GetClassProperties(type))
        {
            var propType = prop.Type;
            il.Emit(OpCodes.Ldarg_0);//Load arg Serializer
            il.Emit(OpCodes.Ldarg_1);//Load stream
            il.Emit(OpCodes.Ldarg_2);//Load seriliaze function 

            //(current serialzier instance).Write(stream, GetMethod())
            il.Emit(OpCodes.Call, prop.GetMethod);
            il.Emit(OpCodes.Call, ser.GetMethodForSerialize(seriliazeMethod, propType, type));
        }

        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Ret);
        return seriliazeMethod;

    }

    public static MethodInfo CreateDeserializerMethod(Serializer ser, KAType type)
    {
        //Criar um método estático
        DynamicMethod deserMethod = new DynamicMethod(
            $"Deserialize for class type: {type.FullName}",
            type.DType,
            [typeof(Serializer), typeof(Stream)]

        );

        ConstructorInfo? emptyConstructor = type.DType.GetConstructor(Type.EmptyTypes);

        if (emptyConstructor == null)
        {
            throw new Exception($"There is no empty constuctor for the type {type.FullName}");
        }

        //Entra Serializer(estancia) Stream
        var il = deserMethod.GetILGenerator();

        il.DeclareLocal(type.DType);//Criar var para Stloc_0

        il.Emit(OpCodes.Nop);

        il.Emit(OpCodes.Newobj, emptyConstructor);
        il.Emit(OpCodes.Stloc_0);//Save object criado para Stloc_0

        foreach (var prop in GetClassProperties(type))
        {
            var propType = prop.Type;

            //estático não têm inscancia. Se tivese, a intancia desse mesmo objecto estaria em 0, por isso os "verdadeiros" arugmentos comçam em 1 inclusive.
            il.Emit(OpCodes.Ldloc_0);//Carregar objecto para fazer deser
            il.Emit(OpCodes.Ldarg_0);//Arg do serializer
            il.Emit(OpCodes.Ldarg_1);//Arg do stream

            il.Emit(OpCodes.Call, ser.GetMethodForDeserialize(deserMethod, propType, type));

            il.Emit(OpCodes.Callvirt, prop.SetMethod!);

            il.Emit(OpCodes.Nop);
        }

        //Load object local 0(objecto para fazer deser)
        il.Emit(OpCodes.Ldloc_0);

        //Return
        il.Emit(OpCodes.Ret);

        return deserMethod;
    }

    public static List<KAClassProperty> GetClassProperties(KAType type)
    {
        var props = new List<KAClassProperty>();

        foreach (var prop in type.DType.GetProperties().ToList().OrderBy((p) => p.Name))
        {
            if (prop.GetMethod == null || prop.SetMethod == null)
            {
                throw new Exception($"The prop '{prop.Name}' of the class '{type.FullName}' has the get or set not public!");
            }
                if (Attribute.IsDefined(prop, typeof(IgnoreParse)) )
                {
                    continue;
                }
                
            var propType = prop.PropertyType;

            props.Add(new()
            {
                Name = prop.Name,
                Type = KAType.Get(propType),
                GetMethod = prop.GetMethod,
                SetMethod = prop.SetMethod
            });

        }
        return props;
    }

   

}


