using System.Text.Json;
using Kernel.AFCP;
using Kernel.Modules.Base;

public class ConfigParser
{
    public static ConfigFile.Config Load(string FileName)
    {
        string jsonString = File.ReadAllText(FileName);
        ConfigFile.Config config = JsonSerializer.Deserialize<ConfigFile.Config>(jsonString)!;

        for (int i = 0; i < config.ModulesDlls.Length; i++)
        {
            var summonPath = Directory.GetCurrentDirectory();
            config.ModulesDlls[i] = Path.Join(summonPath, config.ModulesDlls[i].Replace("[root]", ""));
        }
        return config;
    }

}


namespace ConfigFile
{

    public partial class Config
    {
        public required string[] ModulesDlls { get; set; }

        public required StartupModule[] StartupModules { get; set; }
    }

    public partial class StartupModule
    {
        public required string ModuleName { get; set; }

        public required string Path { get; set; }
        public bool StartOnBoot { get; set; }
        public string? startAfter { get; set; }
        public List<ConfigChannelDescriptor>? hostingChannels { get; set; }
        public List<ConfigChannelDescriptor>? connectedChannels { get; set; }

        public Dictionary<string, object>? Config { get; set; }

    }

}
