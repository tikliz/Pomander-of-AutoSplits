using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;

using PomanderoSplit.Utils;

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

    public unsafe override void Draw()
    {
        // livesplit port input
        var portHasChanged = portValue != Plugin.Configuration.LivesplitPort;
        ImGui.BeginDisabled(!portHasChanged);
        if (ImGui.Button("Save"))
        {
            Plugin.Configuration.LivesplitPort = portValue;
            Plugin.Configuration.Save();

            // Plugin.ConnectionManager.ChangePort(portValue);
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        bool useTCP = false; // Plugin.ConnectionManager.UseTCP;
        if (ImGui.Checkbox("Use TCP?", ref useTCP))
        {
            Plugin.Configuration.UseTCP = useTCP;
            // Plugin.ConnectionManager.ChangeTCP(useTCP);
            Plugin.Configuration.Save();
        }
        ImGui.SameLine();
        ImGui.BeginDisabled(!useTCP);
        ImGui.InputInt("LiveSplit Port", ref portValue, 0);
        ImGui.EndDisabled();

        ImGui.BeginDisabled(Plugin.ConnectionManager.Status() == Connection.ClientStatus.Connected);
        if (ImGui.Button("CONNECT"))
        {
            Plugin.ConnectionManager.Connect();
            Plugin.Configuration.Connect = true;
            Plugin.Configuration.Save();

            // Task.Run(Plugin.LiveSplitClient.Connect);
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        ImGui.BeginDisabled(Plugin.ConnectionManager.Status() == Connection.ClientStatus.Disconnected);
        if (ImGui.Button("CLOSE CONNECTION"))
        {
            Plugin.ConnectionManager.Disconnect();
            Plugin.Configuration.Connect = false;
            Plugin.Configuration.Save();
            
            // Task.Run(Plugin.LiveSplitClient.Disconnect);
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("SEND PLAY"))
        {
            Plugin.ConnectionManager.Resume();
        }
        ImGui.SameLine();
        if (ImGui.Button("SEND STOP"))
        {
            Plugin.ConnectionManager.Pause();
        }
        ImGui.SameLine();
        if (ImGui.Button("TEST"))
        {
            foreach (var flag in Enum.GetValues(typeof(ConditionFlag)).Cast<ConditionFlag>())
            {
                if (Dalamud.Conditions[flag])
                {
                    Dalamud.Log.Info($"{flag} - {Dalamud.Conditions[flag]}");
                }
            }
            Dalamud.Log.Info("\n -------------------------------- \n");
            var instance = EventFramework.Instance();
            var deep_dungeon_data = instance->GetInstanceContentDeepDungeon();
            Dalamud.Log.Info($"Floor: {deep_dungeon_data->Floor}");
            LogHelper.ReportInfo("teste");
        }
        ImGui.SameLine();
        Helpers.RightAlign(40.0f);

        Widget.StatusCircle(Plugin.ConnectionManager.Status() == Connection.ClientStatus.Connected);
    }
}
