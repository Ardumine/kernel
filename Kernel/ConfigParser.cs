using System.Text.Json;
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

        public required StartupModuleConfig[] StartupModules { get; set; }
    }

    public partial class StartupModuleConfig
    {
        public required string ModuleName { get; set; }

        public required string Path { get; set; }
        public bool StartOnBoot { get; set; }
        public string? startAfter { get; set; }
        public List<ConfigChannelDescriptor> hostingChannels { get; set; } = new();
        public List<ConfigChannelDescriptor> connectedChannels { get; set; } = new();

        public Dictionary<string, object>? Config { get; set; }

    }

}
public class ConfigChannelDescriptor
{
    public required string Path { get; set; }
    public required string Name { get; set; }
    public bool HighData { get; set; }
}