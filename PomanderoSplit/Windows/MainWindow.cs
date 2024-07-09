using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using PomanderoSplit.Utils;

namespace PomanderoSplit.Windows;

public class MainWindow : Window, IDisposable
{
    private static int indexRun = 0;
    
    private Plugin Plugin { get; init; }

    public MainWindow(Plugin plugin) : base("PomanderoSplit")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Plugin = plugin;
    }

    public override void Draw()
    {
        DrawIndexArrows();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var data = Plugin.GenericRunManager.Runs[indexRun];
        
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

    public void Dispose() { }

    private void DrawIndexArrows()
    {
        var size = Plugin.GenericRunManager.Runs.Count;
        if (size < indexRun) indexRun = size;

        float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
        ImGui.PushButtonRepeat(true);
        if (ImGui.ArrowButton("##left", ImGuiDir.Left))
        {
            if (indexRun != 0) indexRun--;
        }
        ImGui.SameLine(0.0f, spacing);
        if (ImGui.ArrowButton("##right", ImGuiDir.Right))
        {
            if (size != indexRun) indexRun++;
        }
        ImGui.PopButtonRepeat();
        ImGui.SameLine();
        ImGui.Text($"index {indexRun} | size {size}");
    }
}
