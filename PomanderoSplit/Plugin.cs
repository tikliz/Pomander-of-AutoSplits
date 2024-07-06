using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using PomanderoSplit.Windows;
using System.Collections.Immutable;
using System.Collections.Generic;
using System;
using Dalamud.Interface.Style;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.Event;

namespace PomanderoSplit;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;

    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static INotificationManager notificationManager { get; private set; } = null!;
    [PluginService] public static IClientState clientState { get; private set; } = null!;
    [PluginService] public static ICondition conditionState { get; private set; } = null!;

    [PluginService] public static IDutyState dutyState { get; private set; } = null!;

    // private static ImmutableArray<string> CommandSubs = 

    public Configuration Configuration { get; set; }

    public static LiveSplitClient LiveSplitClient { get; set; } = null!;

    public static EventSubscribers EventSubscribers { get; set; } = null!;

    // public LogHelper LogHelper { get; init; }

    public CommandHandler CommandHandler { get; set; }

    public readonly WindowSystem WindowSystem = new("PomanderoSplit");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        LiveSplitClient = new LiveSplitClient(this, ChatGui, notificationManager);
        CommandHandler = new CommandHandler(this, CommandManager, ChatGui);
        EventSubscribers = new EventSubscribers(clientState, ChatGui, LiveSplitClient, conditionState, dutyState);

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);
        // LogHelper = new LogHelper();

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        LiveSplitClient.Disconnect();
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        EventSubscribers.Dispose();
        CommandHandler.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        List<string> split_args = [.. args.Split(' ')];
        ToggleMainUI();
    }

    private void OnSettings(string command, string args)
    {
        ToggleConfigUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI()
    {
        ConfigWindow.Toggle();
        Log.Info($"Opening config. LiveSplit port is {LiveSplitClient.port}");
    }
    public void ToggleMainUI() => MainWindow.Toggle();
    
}
