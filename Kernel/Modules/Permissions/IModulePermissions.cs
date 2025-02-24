internal class IModulePermissions
{
    private List<ModulePermission> Permissions { get; set; }

    public IModulePermissions(){
        Permissions = new List<ModulePermission>();
    }

    public bool CheckIfHaveMPathPermission(string MPath)
    {
        foreach (var Permission in Permissions)
        {
            if (Permission is ConnectChannelPermission channelPermission)
            {
                if (channelPermission.MPath == MPath)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the real path for a channel that a module has access to.
    /// </summary>
    /// <param name="MPath">The MPath</param>
    /// <returns>If null, then the module does not have access to it. Else, returns the real channel path.</returns>
    public string? SolveMPath(string MPath)
    {
        foreach (var Permission in Permissions)
        {
            if (Permission is ConnectChannelPermission channelPermission)
            {
                if (channelPermission.MPath == MPath)
                {
                    return channelPermission.RealPath;
                }
            }
        }
        return null;
    }

    public void AddPerm(ModulePermission perm){
        Permissions.Add(perm);
    }
}