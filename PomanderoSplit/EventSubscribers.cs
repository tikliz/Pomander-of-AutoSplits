
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace PomanderoSplit;

public class EventSubscribers
{
    private bool resetSplits = false;

    private LiveSplitClient LiveSplitClient { get; init; }

    public EventSubscribers(LiveSplitClient liveSplitClient)
    {
        LiveSplitClient = liveSplitClient;

        Dalamud.ClientState.TerritoryChanged += OnTerritoryChanged;
        Dalamud.Conditions.ConditionChange += OnConditionChange;
        Dalamud.Duty.DutyWiped += OnDutyWiped;
        Dalamud.Duty.DutyCompleted += OnDutyCompleted;
        Dalamud.Duty.DutyStarted += OnDutyStarted;
    }

    private unsafe void OnConditionChange(ConditionFlag flag, bool value)
    {
        if (flag == ConditionFlag.InDeepDungeon && !value)
        {
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Paused splits!"), new UIForegroundPayload(0)));
            LiveSplitClient.Pause();
        }

        if (flag == ConditionFlag.BetweenAreas && Dalamud.Conditions[ConditionFlag.InDeepDungeon])
        {
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Changed floors maybe."), new UIForegroundPayload(0)));
            LiveSplitClient.StartOrSplit();
            LogDeepDungeonData();
        }
    }

    private void OnTerritoryChanged(ushort id)
    {
        if (resetSplits) ResetSplits();
    }

    private void OnDutyWiped(object? sender, ushort e)
    {
        Dalamud.Chat.Print("DUTY WIPED");
        if (!Dalamud.Conditions[ConditionFlag.InDeepDungeon]) return;

        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload("Run ended pause!"), new UIForegroundPayload(0)));
        resetSplits = true;
        LiveSplitClient.Pause();
    }

    private void OnDutyCompleted(object? sender, ushort e)
    {
        if (!Dalamud.Conditions[ConditionFlag.InDeepDungeon]) return;

        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Run finished pause!"), new UIForegroundPayload(0)));
        resetSplits = true;
        LiveSplitClient.Pause();
    }

    private void OnDutyStarted(object? sender, ushort e)
    {
        if (!Dalamud.Conditions[ConditionFlag.InDeepDungeon]) return;

        LiveSplitClient.Reset();
        LiveSplitClient.StartOrSplit();
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Reset and started splits!"), new UIForegroundPayload(0)));
    }
    
    private void ResetSplits()
    {
        resetSplits = false;
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload("Reset!"), new UIForegroundPayload(0)));
        LiveSplitClient.Reset();
    }

    public unsafe InstanceContentDeepDungeon* GetInstanceContentDeepDungeon()
    {
        var instance = EventFramework.Instance();
        return instance->GetInstanceContentDeepDungeon();
    }

    public unsafe void LogDeepDungeonData()
    {
        InstanceContentDeepDungeon* deep_dungeon_data = GetInstanceContentDeepDungeon();

        if (deep_dungeon_data == null)
        {
            Dalamud.Log.Info("--- NULL DEEP DUNGEON DATA ---");
            return;
        }

        Dalamud.Log.Info($"Floor {deep_dungeon_data->Floor}");
        Dalamud.Log.Info($"Recommended level? {deep_dungeon_data->GetRecommendedLevel()}");
        Dalamud.Log.Info($"Passage Progress {deep_dungeon_data->PassageProgress}");
        Dalamud.Log.Info($"Weapon level: {deep_dungeon_data->WeaponLevel}, Armor level: {deep_dungeon_data->ArmorLevel}");
    }

    public void Dispose()
    {
        Dalamud.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Dalamud.Conditions.ConditionChange -= OnConditionChange;
        Dalamud.Duty.DutyWiped -= OnDutyWiped;
        Dalamud.Duty.DutyCompleted -= OnDutyCompleted;
        Dalamud.Duty.DutyStarted -= OnDutyStarted;
    }
}