using System;
using ImGuiNET;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility.Raii;

namespace PomanderoSplit.Windows;

public partial class ConfigWindow : Window, IDisposable
{
    public void DrawDebugTab()
    {
        using var tab = ImRaii.TabItem("Debug");
        if (!tab) return;

        if (ImGui.Button("Send Begin")) Plugin.ConnectionManager.Begin();
        if (ImGui.Button("Send Reset")) Plugin.ConnectionManager.Reset();
        if (ImGui.Button("Send Split")) Plugin.ConnectionManager.Split();
        if (ImGui.Button("Send Pause")) Plugin.ConnectionManager.Pause();
        if (ImGui.Button("Send Resume")) Plugin.ConnectionManager.Resume();

        ImGui.Separator();

        
        
        ImGui.Separator();
        
        if (ImGui.Button("Reset Run"))
        {
            Plugin.GenericRunManager.ResetRun();
        }
        
        if (ImGui.Button("Create Run"))
        {
            Plugin.GenericRunManager.CreateTestRun();
        }

        ImGui.Separator();
        
        ImGui.Text($"{Plugin.GenericRunManager.CurrentRun()}");
        

        if (ImGui.Button("Dummy Button"))
        {
        //    foreach (var flag in Enum.GetValues(typeof(ConditionFlag)).Cast<ConditionFlag>())
        //     {
        //         if (Dalamud.Conditions[flag])
        //         {
        //             Dalamud.Log.Info($"{flag} - {Dalamud.Conditions[flag]}");
        //         }
        //     }
        //     Dalamud.Log.Info("\n -------------------------------- \n");
        //     var instance = EventFramework.Instance();
        //     var deep_dungeon_data = instance->GetInstanceContentDeepDungeon();
        //     Dalamud.Log.Info($"Floor: {deep_dungeon_data->Floor}");
        //     LogHelper.ReportInfo("teste");
        }
    }
}