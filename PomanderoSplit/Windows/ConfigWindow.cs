using System;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using PomanderoSplit.Util;

namespace PomanderoSplit.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Plugin Plugin { get; init; }

    private int portValue = 0;

    public ConfigWindow(Plugin plugin) : base("PomanderoSplit Tweaks")
    {
        Plugin = plugin;

        Flags = ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoScrollWithMouse |
            ImGuiWindowFlags.AlwaysAutoResize;
        SizeCondition = ImGuiCond.Once;

        portValue = Plugin.Configuration.LivesplitPort;
    }

    public void Dispose() { }

    // fake ? nao lembro ter precisado fazer isso "Flags must be added or removed before Draw() is being called, or they won't apply"
    public override void PreDraw()
    {
        if (Plugin.Configuration.IsConfigWindowMovable) Flags &= ~ImGuiWindowFlags.NoMove;
        else Flags |= ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        // livesplit port input
        var portHasChanged = portValue != Plugin.Configuration.LivesplitPort;
        ImGui.BeginDisabled(!portHasChanged);
        if (ImGui.Button("Save"))
        {
            Plugin.Configuration.LivesplitPort = portValue;
            Plugin.Configuration.Save();

            Plugin.LiveSplitClient.ChangePort(portValue);
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        ImGui.InputInt("LiveSplit Port", ref portValue, 0);

        // connection test buttons

        ImGui.BeginDisabled(Plugin.LiveSplitClient.Status());
        if (ImGui.Button("CONNECT"))
        {
            Plugin.LiveSplitClient.Connect();

            // Task.Run(Plugin.LiveSplitClient.Connect);
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        ImGui.BeginDisabled(!Plugin.LiveSplitClient.Status());
        if (ImGui.Button("CLOSE CONNECTION"))
        {
            Plugin.LiveSplitClient.Disconnect();
            
            // Task.Run(Plugin.LiveSplitClient.Disconnect);
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("SEND PLAY"))
        {
            Plugin.LiveSplitClient.StartOrSplit();
        }
        ImGui.SameLine();
        Helpers.RightAlign(40.0f);

        Widget.StatusCircle(Plugin.LiveSplitClient.Status());
    }
}
