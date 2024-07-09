using Dalamud.Configuration;
using System;

using PomanderoSplit.Connection;

namespace PomanderoSplit;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool IsConnectedLivesplit { get; set; } = false;
    public int LivesplitPort { get; set; } = 16834;
    public bool UseTCP { get; set; } = false;
    public bool Connect { get; set; } = false;

    public bool AutoConnect { get; set; } = true;
    public ClientType ClientType { get; set; } = ClientType.Pipe;
    public string Address { get; set; } = "localhost:16834"; // nao sei o default address correto chutei esse

    public void Save() => Dalamud.PluginInterface.SavePluginConfig(this);
    
    public static Configuration Load()
    {
        if (Dalamud.PluginInterface.GetPluginConfig() is Configuration config) return config;
        config = new Configuration();
        config.Save();
        return config;
    }
}
