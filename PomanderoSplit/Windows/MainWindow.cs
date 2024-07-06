using System;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using PomanderoSplit.Widgets;

namespace PomanderoSplit.Windows;

public class MainWindow : Window, IDisposable
{
    private string goatImagePath;
    private Plugin plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string goatImagePath)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.goatImagePath = goatImagePath;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            plugin.ToggleConfigUI();
        }
        ImGui.SameLine();
        WidgetHelpers.RightAlign(40.0f);
        StatusCircle.Draw();

        ImGui.Spacing();
        // ImGui.Text("Have a goat:");
        // var goatImage = Plugin.TextureProvider.GetFromFile(goatImagePath).GetWrapOrDefault();
        // if (goatImage != null)
        // {
        //     ImGuiHelpers.ScaledIndent(55f);
        //     ImGui.Image(goatImage.ImGuiHandle, new Vector2(goatImage.Width, goatImage.Height));
        //     ImGuiHelpers.ScaledIndent(-55f);
        // }
        // else
        // {
        //     ImGui.Text("Image not found.");
        // }

        ImGui.TreeNode("Splits");
    }
}
