using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using PomanderoSplit.Utils;

namespace PomanderoSplit.Windows;

public partial class MainWindow : Window, IDisposable
{
    private static int IndexRun = 0;
    
    private Plugin Plugin { get; init; }


    public MainWindow(Plugin plugin) : base("PomanderoSplit")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Plugin = plugin;
    }

    public override void Draw()
    {
        using var bar = ImRaii.TabBar("Main Window tabs");
        if (!bar) return;

        DrawMainTab();

        #if DEBUG
            DrawDebugTab();
        #endif
    }

    public void Dispose() { }

}
