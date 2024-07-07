using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using PomanderoSplit.Utils;

namespace PomanderoSplit.Windows;

public class MainWindow : Window, IDisposable
{
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

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings")) Plugin.ConfigWindow.Toggle();

        ImGui.SameLine();
        Helpers.RightAlign(40.0f);

        Widget.StatusCircle(Plugin.LiveSplitClient.Status());

        ImGui.Spacing();

        ImGui.TreeNode("Splits");
    }
}
