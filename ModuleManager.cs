using System.Reflection;

namespace Ardumine.Module;

class ModuleManager
{
    public static List<Module> GetModules()
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        List<Module> mods = new();

        foreach (Type t in types)
        {
            if (t.Namespace != null)
            {
                if (t.Namespace.StartsWith("Ardumine.Module"))
                {
                    if (t.IsSubclassOf(typeof(Module)))
                    {
                        mods.Add((Module)Activator.CreateInstance(t));
                    }
                }
            }
        }


        return mods;
    }
}

