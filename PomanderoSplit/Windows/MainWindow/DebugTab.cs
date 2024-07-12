using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using PomanderoSplit.Utils;

namespace PomanderoSplit.Windows;

public partial class MainWindow : Window, IDisposable
{

    public void DrawDebugTab()
    {
        using var tab = ImRaii.TabItem("Debug view");
        if (!tab) return;

        DrawIndexArrows();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var data = Plugin.GenericRunManager.Runs[IndexRun];
        
        var text = $"Run {data.Name}";
        ImGui.Text(text);
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        foreach (var objective in data.Objectives)
        {
            ImGui.Text($"{objective.Name}");
        }
        
        ImGui.Separator();
        
        foreach (var split in data.Splits)
        {
            ImGui.Text($"{split.ToString()}"); // "HH:MM:SS:ss"
        }
    }

    private void DrawIndexArrows()
    {
        var size = Plugin.GenericRunManager.Runs.Count;
        if (size < IndexRun) IndexRun = size;

        float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
        ImGui.PushButtonRepeat(true);
        if (ImGui.ArrowButton("##debug_left", ImGuiDir.Left))
        {
            if (IndexRun != 0) IndexRun--;
        }
        ImGui.SameLine(0.0f, spacing);
        if (ImGui.ArrowButton("##debug_right", ImGuiDir.Right))
        {
            if (size != IndexRun) IndexRun++;
        }
        ImGui.PopButtonRepeat();
        ImGui.SameLine();
        ImGui.Text($"index {IndexRun} | size {size}");
    }
}
