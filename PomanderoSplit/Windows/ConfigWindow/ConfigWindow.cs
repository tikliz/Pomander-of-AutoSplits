using System;
using ImGuiNET;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility.Raii;

namespace PomanderoSplit.Windows;

public partial class ConfigWindow : Window, IDisposable
{
    private Plugin Plugin { get; init; }

    private string addressValue = string.Empty;

    public ConfigWindow(Plugin plugin) : base("PomanderoSplit Tweaks")
    {
        Plugin = plugin;

        Flags = ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoScrollWithMouse |
            ImGuiWindowFlags.AlwaysAutoResize;
        SizeCondition = ImGuiCond.Once;

        addressValue = Plugin.Configuration.Address;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var bar = ImRaii.TabBar("Settings Tabs");
        if (!bar) return;

        DrawConnectionTab();
        
        #if DEBUG
            DrawDebugTab();
        #endif
    }
}