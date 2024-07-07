using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace PomanderoSplit;

public class Dalamud
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static ICommandManager Commands { get; private set; } = null!;
    [PluginService] public static IDataManager GameData { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IChatGui Chat { get; private set; } = null!;
    [PluginService] public static ICondition Conditions { get; private set; } = null!;
    [PluginService] public static ITextureProvider Textures { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IPartyList PartyList { get; private set; } = null!;
    [PluginService] public static IDutyState Duty { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static INotificationManager Notifications { get; private set; } = null!;
    //[PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
    //[PluginService] public static IKeyState Keys { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    //[PluginService] public static ITargetManager Targets { get; private set; } = null!;
    //[PluginService] public static IGameInteropProvider Interop { get; private set; } = null!;

    public static void Initialize(IDalamudPluginInterface pluginInterface) => pluginInterface.Create<Dalamud>();
}
