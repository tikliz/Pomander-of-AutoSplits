using System;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace PomanderoSplit;

public class EventSubscribers
{
    private bool reset_splits = false;
    private IClientState clientState;
    private IChatGui chatGui;
    private LiveSplitClient liveSplitClient;
    private ICondition conditionState;
    private ConditionFlag previousFlag;
    private IDutyState dutyState;

    public EventSubscribers(IClientState clientState, IChatGui chatGui, LiveSplitClient liveSplitClient, ICondition conditionState, IDutyState dutyState)
    {
        this.dutyState = dutyState;
        this.conditionState = conditionState;
        this.clientState = clientState;
        this.chatGui = chatGui;
        this.liveSplitClient = liveSplitClient;
        this.clientState.TerritoryChanged += OnTerritoryChanged;
        this.conditionState.ConditionChange += OnConditionChange;
        this.dutyState.DutyWiped += OnDutyWiped;
        this.dutyState.DutyCompleted += OnDutyCompleted;
        this.dutyState.DutyStarted += OnDutyStarted;
    }

    private unsafe void OnConditionChange(ConditionFlag flag, bool value)
    {
        if (flag == ConditionFlag.InDeepDungeon)
        {
            if (!value)
            {
                chatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Paused splits!"), new UIForegroundPayload(0)));
                liveSplitClient.SendMessage("pause");
            }
            
        }

        if (flag == ConditionFlag.BetweenAreas && conditionState[ConditionFlag.InDeepDungeon])
        {
            chatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Changed floors maybe."), new UIForegroundPayload(0)));
            liveSplitClient.SendMessage("startorsplit");
            LogDeepDungeonData();
        }
    }

    private void OnTerritoryChanged(ushort id)
    {
        if (reset_splits)
        {
            ResetSplits();
        }
    }

    private void ResetSplits()
    {
            reset_splits = false;
            chatGui.Print(new SeString(new UIForegroundPayload(73), new TextPayload("Reset!"), new UIForegroundPayload(0)));
            this.liveSplitClient.SendMessage("reset");
    }

    private void OnDutyWiped(object? sender, ushort e)
    {
        chatGui.Print("DUTY WIPED");
        if (conditionState[ConditionFlag.InDeepDungeon])
        {
            chatGui.Print(new SeString(new UIForegroundPayload(73), new TextPayload("Run ended pause!"), new UIForegroundPayload(0)));
            reset_splits = true;
            liveSplitClient.SendMessage("pause");
        }
    }
    private void OnDutyCompleted(object? sender, ushort e)
    {
        if (conditionState[ConditionFlag.InDeepDungeon])
        {
            chatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Run finished pause!"), new UIForegroundPayload(0)));
            reset_splits = true;
            liveSplitClient.SendMessage("pause");
        }
    }

    private void OnDutyStarted(object? sender, ushort e)
    {
        if (conditionState[ConditionFlag.InDeepDungeon])
        {
            liveSplitClient.SendMessage("reset");
            liveSplitClient.SendMessage("startorsplit");
            chatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Reset and started splits!"), new UIForegroundPayload(0)));
        }
    }

    public unsafe InstanceContentDeepDungeon* GetInstanceContentDeepDungeon()
    {
        var instance = EventFramework.Instance();
        return instance->GetInstanceContentDeepDungeon();
    }

    public unsafe void LogDeepDungeonData()
    {
        InstanceContentDeepDungeon* deep_dungeon_data = GetInstanceContentDeepDungeon();
        if (deep_dungeon_data != null)
        {
            Plugin.Log.Info($"Floor {deep_dungeon_data->Floor}");
            Plugin.Log.Info($"Recommended level? {deep_dungeon_data->GetRecommendedLevel()}");
            Plugin.Log.Info($"Passage Progress {deep_dungeon_data->PassageProgress}");
            Plugin.Log.Info($"Weapon level: {deep_dungeon_data->WeaponLevel}, Armor level: {deep_dungeon_data->ArmorLevel}");
        }
        else
        {
            Plugin.Log.Info("--- NULL DEEP DUNGEON DATA ---");
        }
    }

    public void Dispose()
    {
        clientState.TerritoryChanged -= OnTerritoryChanged;
        conditionState.ConditionChange -= OnConditionChange;
        dutyState.DutyWiped -= OnDutyWiped;
        dutyState.DutyCompleted -= OnDutyCompleted;
        dutyState.DutyStarted -= OnDutyStarted;
    }
}