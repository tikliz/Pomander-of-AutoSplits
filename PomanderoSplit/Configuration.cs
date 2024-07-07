using Dalamud.Configuration;
using System;

namespace PomanderoSplit;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool IsConnectedLivesplit { get; set; } = false;
    public int LivesplitPort { get; set; } = 16834;

    public void Save() => Dalamud.PluginInterface.SavePluginConfig(this);
    
    public static Configuration Load()
    {
        if (Dalamud.PluginInterface.GetPluginConfig() is Configuration config) return config;
        config = new Configuration();
        config.Save();
        return config;
    }
}
