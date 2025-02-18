using System.Diagnostics.CodeAnalysis;

public struct KAType
{
    public required Type DType { get; set; }


    public required KASubTypes TypeOfData { get; set; }
    public required bool IsUnamanged { get; set; }
    public required bool CanBeNull { get; set; }
    public required bool CanBeDerived { get; set; }


    /// <summary>
    /// If the type is a array or a dict, the holding types are stored here.
    /// 
    /// - If the type is an 1D Array or List, the [0] represents the value type.
    /// - If the type is an Dict, then [0] represents the key type and [1] represents value type.
    /// </summary>
    public KAType[] SubHoldingType { get; set; }


    public required string? FullName { get; set; }
    public required string? AssemblyName { get; set; }



    public static KAType Get(Type type)
    {
        var typeofData = KASubTypes.UnmanagedStruct;
        List<KAType> HoldingTypes = new();


        if (type == typeof(string))
        {
            typeofData = KASubTypes.String;
        }
        else if (SerializerTools.IsList(type))
        {
            var listItemType = type.GetGenericArguments().Single();

            if (SerializerTools.IsUnmanagedStruct(listItemType))
            {
                typeofData = KASubTypes.UnmanagedList;
                HoldingTypes.Add(Get(listItemType));
            }
            else
            {
                typeofData = KASubTypes.ManagedList;
                HoldingTypes.Add(Get(listItemType));
            }
        }
        else if (SerializerTools.IsSpan(type))
        {
            throw new Exception($"The type {type} is a span. Spans cannot be serialized. Use an array to hold your data.");
            /*var spanItemType = type.GetGenericArguments().Single();
            if (SerializerTools.IsUnmanagedStruct(spanItemType))
            {
                //typeofData = KASubTypes.UnamanagedSpan;
                HoldingType = Get(spanItemType);
            }*/
        }
        else if (type.IsArray && type.GetArrayRank() == 1)
        {
            Type arrayType = type.GetElementType()!;

            if (SerializerTools.IsUnmanagedStruct(arrayType))
            {
                typeofData = KASubTypes.Unmanaged1DArray;
                HoldingTypes.Add(Get(arrayType));
            }
            else
            {
                typeofData = KASubTypes.Managed1DArray;
                HoldingTypes.Add(Get(arrayType));
            }
        }
        else if (type == typeof(Guid))
        {
            typeofData = KASubTypes.Guid;
        }
        else if (SerializerTools.IsUnmanagedStruct(type))
        {
            typeofData = KASubTypes.UnmanagedStruct;
        }
        else if (SerializerTools.IsDict(type))
        {
            //https://stackoverflow.com/questions/16956903/determine-if-type-is-dictionary

            typeofData = KASubTypes.Dict;
            HoldingTypes.Add(Get(type.GetGenericArguments()[0]));
            HoldingTypes.Add(Get(type.GetGenericArguments()[1]));
        }
        else if (SerializerTools.IsManagedStruct(type))
        {
            typeofData = KASubTypes.UnmanagedStruct;
        }
        else if (type.IsClass)
        {
            typeofData = KASubTypes.Class;
        }
        else
        {
            throw new Exception($"Cannot get the general type of the type {type.Name}");
        }


        return new()
        {
            DType = type,
            TypeOfData = typeofData,
            CanBeNull = SerializerTools.CanBeNull(type),
            SubHoldingType = HoldingTypes.ToArray(),
            FullName = type.FullName,
            IsUnamanged = SerializerTools.IsUnmanagedStruct(type),
            AssemblyName = type.AssemblyQualifiedName,
            CanBeDerived = SerializerTools.CanBeDerived(type),
        };
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is KAType @ob)
        {
            return DType.Equals(@ob.DType);
        }
        return base.Equals(obj);
    }
    public static bool operator ==(KAType left, KAType right)
    {
        return left.DType.Equals(right.DType);
    }
    public static bool operator !=(KAType left, KAType right)
    {
        return !left.DType.Equals(right.DType);
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}

public enum KASubTypes
{
    String,
    Guid,
    UnmanagedStruct,//int, float, long, bool, byte, unmanaged structs, etc...
    ManagedStruct,
    Class,
    UnmanagedList,
    ManagedList,
    Unmanaged1DArray,
    Managed1DArray,
    Dict
}
