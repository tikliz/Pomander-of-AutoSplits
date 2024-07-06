using System;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using PomanderoSplit.Widgets;

namespace PomanderoSplit.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;

    // private LogHelper logHelper;

    private int socketValue = 0;
    public ConfigWindow(Plugin plugin) : base("PomanderoSplit Tweaks")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize;

        // Size = new Vector2(0, 0);
        SizeCondition = ImGuiCond.Once;
        
        configuration = plugin.Configuration;
        socketValue = configuration.LivesplitPort;
        // logHelper = plugin.LogHelper;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        // livesplit port input
        bool port_changed = socketValue != configuration.LivesplitPort;
        ImGui.BeginDisabled(!port_changed);
        if (ImGui.Button("Save"))
        {
            configuration.LivesplitPort = socketValue;
            Plugin.LiveSplitClient?.UpdatePort();
            configuration.Save();
            Plugin.LiveSplitClient?.Reconnect();
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        ImGui.InputInt("LiveSplit Port", ref socketValue, 0);

        // connection test buttons
        bool connected = Plugin.LiveSplitClient != null && Plugin.LiveSplitClient.livesplitSocket.Connected;
        ImGui.BeginDisabled(connected);
        if (ImGui.Button("CONNECT"))
        {
            Plugin.LiveSplitClient?.Connect();
        }
        ImGui.EndDisabled();
        
        ImGui.SameLine();
        ImGui.BeginDisabled(!connected);
        if (ImGui.Button("CLOSE CONNECTION"))
        {
            Plugin.LiveSplitClient?.Disconnect();
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("SEND PLAY"))
        {
            Plugin.LiveSplitClient?.SendMessage("startorsplit");
        }
        ImGui.SameLine();
        WidgetHelpers.RightAlign(40.0f);
        StatusCircle.Draw();
    }

    
}
