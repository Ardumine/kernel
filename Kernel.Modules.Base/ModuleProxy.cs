using AFCP;

namespace Kernel.Modules.Base;

public class ModuleProxy<T> : System.Reflection.DispatchProxy where T : class, IModuleInterface
{
    public required Dictionary<string, uint> CacheMethods { get; set; }
    public required ModuleChannel channel { get; set; }
    protected override object? Invoke(System.Reflection.MethodInfo? targetMethod, object?[]? args)
    {
        return channel.Run(CacheMethods[targetMethod!.Name], args);
    }

    private IEnumerable<System.Reflection.MethodInfo> GetMethods(Type type)
    {
        System.Reflection.MethodInfo[] array = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static |
                                                   System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly);
        for (int i = 0; i < array.Length; i++)
        {
            System.Reflection.MethodInfo? method = array[i];
            yield return method;
        }
        if (type.IsInterface)
        {
            Type[] array1 = type.GetInterfaces();
            for (int i = 0; i < array1.Length; i++)
            {
                Type? iface = array1[i];
                foreach (var method in GetMethods(iface))
                {
                    yield return method;
                }
            }
        }
    }
    Dictionary<string, uint> CreateCache(Type t)
    {
        Dictionary<string, uint> methods = new();
        uint I = 0;
        foreach (var item in GetMethods(t))
        {
            methods[item.Name] = I++;
        }
        return methods;
    }

#pragma warning disable CS0693 // O parâmetro de tipo tem o mesmo nome que o parâmetro de tipo do tipo externo
    public static T CreateProxy<T>(ModuleChannel moduleChannel) where T : class, IModuleInterface
    {
        ModuleProxy<T> proxy = (Create<T, ModuleProxy<T>>() as ModuleProxy<T>)!;
        var type = typeof(T);
        proxy.CacheMethods = proxy.CreateCache(typeof(T));
        proxy.channel = moduleChannel;
        return (proxy as T)!;
    }
#pragma warning restore CS0693 // O parâmetro de tipo tem o mesmo nome que o parâmetro de tipo do tipo externo


}
