using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kernel.AFCP.KASerializer;
///https://stackoverflow.com/questions/53968920/how-do-i-check-if-a-type-fits-the-unmanaged-constraint-in-c
public static class UnmanagedType
{
    private static Dictionary<Type, bool> cachedTypes =new ();
    
    public static bool IsUnManaged(this Type t)
    {
        var result = false;
        if (cachedTypes.ContainsKey(t))
            return cachedTypes[t];
        else if (t.IsPrimitive || t.IsPointer || t.IsEnum)
            result = true;
        else if (t.IsGenericType || !t.IsValueType)
            result = false;
        else
            result = t.GetFields(BindingFlags.Public |
               BindingFlags.NonPublic | BindingFlags.Instance)
                .All(x => x.FieldType.IsUnManaged());
        cachedTypes.Add(t, result);
        return result;
    }
}