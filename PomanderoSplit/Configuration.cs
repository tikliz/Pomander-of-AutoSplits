using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Net.Sockets;
using System;

namespace PomanderoSplit;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    public bool IsConnectedLivesplit = false;

    public int LivesplitPort { get; set; } = 16834;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
