public class ConnectChannelPermission : ModulePermission
{
    /// <summary>
    /// The actual channel path
    /// </summary>
    public required string RealPath { get; set; }
    
    /// <summary>
    /// The MPath
    /// </summary>
    public required string MPath { get; set; }
}