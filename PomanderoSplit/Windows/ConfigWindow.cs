using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
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

            Plugin.LiveSplitClient.ChangePort(portValue);
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        bool useTCP = Plugin.LiveSplitClient.UseTCP;
        if (ImGui.Checkbox("Use TCP?", ref useTCP))
        {
            Plugin.Configuration.UseTCP = useTCP;
            Plugin.LiveSplitClient.ChangeTCP(useTCP);
            Plugin.Configuration.Save();
        }
        ImGui.SameLine();
        ImGui.BeginDisabled(!useTCP);
        ImGui.InputInt("LiveSplit Port", ref portValue, 0);
        ImGui.EndDisabled();

        ImGui.BeginDisabled(Plugin.LiveSplitClient.Status());
        if (ImGui.Button("CONNECT"))
        {
            Plugin.LiveSplitClient.Connect();
            Plugin.Configuration.Connect = true;
            Plugin.Configuration.Save();

            // Task.Run(Plugin.LiveSplitClient.Connect);
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        ImGui.BeginDisabled(!Plugin.LiveSplitClient.Status());
        if (ImGui.Button("CLOSE CONNECTION"))
        {
            Plugin.LiveSplitClient.Disconnect();
            Plugin.Configuration.Connect = false;
            Plugin.Configuration.Save();
            
            // Task.Run(Plugin.LiveSplitClient.Disconnect);
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("SEND PLAY"))
        {
            Plugin.LiveSplitClient.StartOrSplit();
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

        Widget.StatusCircle(Plugin.LiveSplitClient.Status());
    }

    // test stuff
    public unsafe static int SaveSlotNumber(IGameGui gameGui)
    {
        static AtkResNode* GetSlotNode(AtkUnitBase* addon, int index)
        {
            var componentNode = GetAddonChildNode(addon, 2)->GetAsAtkComponentNode();
            componentNode = GetComponentChildNode(componentNode, index)->GetAsAtkComponentNode();
            return GetComponentChildNode(componentNode, 1);
        }

        var saveSlotNumber = 0;
        var addon = (AtkUnitBase*)gameGui?.GetAddonByName("DeepDungeonSaveData", 1)!;
        if (addon == null)
            return saveSlotNumber;

        var slot1Node = GetSlotNode(addon, 1);
        var slot2Node = GetSlotNode(addon, 2);

        if (slot1Node != null && slot2Node != null)
        {
            var r1 = slot1Node->AddRed;
            var r2 = slot2Node->AddRed;

            if (r1 > r2)
                saveSlotNumber = 1;
            else if (r2 > r1)
                saveSlotNumber = 2;
        }
        return saveSlotNumber;
    }

    private unsafe static AtkResNode* GetAddonChildNode(AtkUnitBase* addon, int index)
    {
        if (addon == null)
            return null;
        return (index < addon->UldManager.NodeListCount) ? addon->UldManager.NodeList[index] : null;
    }

    private unsafe static AtkResNode* GetComponentChildNode(AtkComponentNode* componentNode, int index)
    {
        if (componentNode == null)
            return null;
        return (index < componentNode->Component->UldManager.NodeListCount) ? componentNode->Component->UldManager.NodeList[index] : null;
    }
}
