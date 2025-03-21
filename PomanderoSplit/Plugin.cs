﻿using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using PomanderoSplit.Connection;
using PomanderoSplit.RunHandler;
using PomanderoSplit.PresetRuns;
using PomanderoSplit.Windows;
using Dalamud.Interface.ImGuiFileDialog;

namespace PomanderoSplit;

public sealed class Plugin : IDalamudPlugin
{
    public Configuration Configuration { get; set; }

    public CommandHandler CommandHandler { get; private init; }
    public ConnectionManager ConnectionManager { get; private init; }
    public GenericRunManager GenericRunManager { get; private init; }
    public PresetRunHandler PresetRunHandler { get; private init; }
    public FileDialogManager FileDialogManager { get; private init; }

    public ConfigWindow ConfigWindow { get; private init; }
    public MainWindow MainWindow { get; private init; }

    private WindowSystem WindowSystem { get; init; } = new("PomanderoSplit");


    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        try
        {
            Dalamud.Initialize(pluginInterface);
            Configuration = Configuration.Load();

            ConnectionManager = new(this);
            CommandHandler = new(this);
            GenericRunManager = new(this);
            PresetRunHandler = new();
            FileDialogManager = new()
            {
                AddedWindowFlags = ImGuiNET.ImGuiWindowFlags.NoCollapse | ImGuiNET.ImGuiWindowFlags.NoDocking,
            };

            ConfigWindow = new(this);
            MainWindow = new(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            
            Dalamud.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
            Dalamud.PluginInterface.UiBuilder.OpenConfigUi += ConfigWindow.Toggle;
            Dalamud.PluginInterface.UiBuilder.OpenMainUi += MainWindow.Toggle;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (MainWindow != null) Dalamud.PluginInterface.UiBuilder.OpenMainUi -= MainWindow.Toggle;
        if (ConfigWindow != null) Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= ConfigWindow.Toggle;
        if (WindowSystem != null) Dalamud.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;

        MainWindow?.Dispose();
        ConfigWindow?.Dispose();

        GenericRunManager?.Dispose();
        CommandHandler?.Dispose();
        ConnectionManager?.Dispose();
    }
}