using System.Reflection;

public class SerializerTools
{
    //https://stackoverflow.com/questions/53968920/how-do-i-check-if-a-type-fits-the-unmanaged-constraint-in-c
    //Para verificar se pode fazer memcpy
    public static bool IsUnmanagedStruct(Type type)
    {
        // primitive, pointer or enum -> true
        if (type.IsPrimitive || type.IsPointer || type.IsEnum)
            return true;

        // not a struct -> false
        if (!type.IsValueType)
            return false;

        // otherwise check recursively
        return type
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .All(f => IsUnmanagedStruct(f.FieldType));
    }
  
    public static bool IsManagedStruct(Type type)
    {
        return type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.IsPointer;
    }
    
    //https://stackoverflow.com/questions/8939939/correct-way-to-check-if-a-type-is-nullable
    public static bool CanBeNullOld(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    public static bool CanBeNull(Type type)
    {
        if (!type.IsValueType)
        {
            return true; // Reference types are nullable
        }
        return Nullable.GetUnderlyingType(type) != null;
    }

    public static bool IsList(Type type){
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }
    public static bool IsDict(Type type){
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
    public static bool IsSpan(Type type){
        return type.FullName!.Contains("System.Span");
       // return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Span<>) || type.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>));
    }
    public static bool CanBeDerived(Type type){
        return type.IsClass && !type.IsSealed && !type.IsAbstract;

    }
}