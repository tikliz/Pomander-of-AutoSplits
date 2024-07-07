using Dalamud.Interface.Windowing;
using Dalamud.Plugin;

using PomanderoSplit.Windows;

namespace PomanderoSplit;

public sealed class Plugin : IDalamudPlugin
{
    public Configuration Configuration { get; set; }

    public CommandHandler CommandHandler { get; private init; }
    public LiveSplitClient LiveSplitClient { get; private init; }
    public EventSubscribers EventSubscribers { get; private init; }

    public ConfigWindow ConfigWindow { get; private init; }
    public MainWindow MainWindow { get; private init; }

    private WindowSystem WindowSystem { get; init; } = new("PomanderoSplit");


    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        try
        {
            Dalamud.Initialize(pluginInterface);
            Configuration = Configuration.Load();

            LiveSplitClient = new(this);
            CommandHandler = new(this);
            EventSubscribers = new(LiveSplitClient);

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

        EventSubscribers?.Dispose();
        CommandHandler?.Dispose();
        LiveSplitClient?.Dispose();
    }
}