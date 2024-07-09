
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using PomanderoSplit.Connection;
using PomanderoSplit.Utils;

namespace PomanderoSplit;

public class EventSubscribers
{
    private ConnectionManager ConnectionManager { get; init; }

    public EventSubscribers(ConnectionManager liveSplitClient)
    {
        ConnectionManager = liveSplitClient;

        Dalamud.ClientState.TerritoryChanged += OnTerritoryChanged;
        Dalamud.Conditions.ConditionChange += OnConditionChange;
        Dalamud.Duty.DutyWiped += OnDutyWiped;
        Dalamud.Duty.DutyCompleted += OnDutyCompleted;
        Dalamud.Duty.DutyStarted += OnDutyStarted;
    }

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        // deep dungeon stuff
        // if (ConnectionManager.onRun)
        // {
        //     if (flag == ConditionFlag.InDeepDungeon && !value)
        //     {
        //         Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Paused splits!"), new UIForegroundPayload(0)));
        //         ConnectionManager.Pause();
        //     }
        //     if (Dalamud.Conditions[ConditionFlag.InDeepDungeon])
        //     {
        //         DeepDungeonHelper.HandleConditions(flag, value, ConnectionManager);
        //     }
        // }


        if (flag == ConditionFlag.Occupied && value)
        {
            Dalamud.Log.Info($"{flag} : {value}");
        }
    }

    private void OnTerritoryChanged(ushort id)
    {
        // if (ConnectionManager.resetSplits || !ConnectionManager.deepDungeonEnd) ConnectionManager.Reset();
    }

    private void OnDutyWiped(object? sender, ushort e)
    {
        Dalamud.Chat.Print("DUTY WIPED");
        // if (!Dalamud.Conditions[ConditionFlag.InDeepDungeon]) return;

        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload("Run ended pause!"), new UIForegroundPayload(0)));
        // ConnectionManager.onRun = false;
        // ConnectionManager.PauseQueueEnd();
    }

    private void OnDutyCompleted(object? sender, ushort e)
    {
        if (Dalamud.Conditions[ConditionFlag.InDeepDungeon])
        {
            // if (DeepDungeonHelper.CheckRunFinished(ConnectionManager))
            // {
            //     Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Run finished pause!"), new UIForegroundPayload(0)));
            //     LogHelper.ReportSuccess("Run finished!");
            //     ConnectionManager.Split();
            //     ConnectionManager.PauseQueueEnd();
            //     ConnectionManager.onRun = false;
            // }
            // else
            // {
            //     Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Floorset finished pause!"), new UIForegroundPayload(0)));
            //     LogHelper.ReportSuccess("Floorset finished pause!");
            //     ConnectionManager.Split();
            //     ConnectionManager.Pause();
            //     ConnectionManager.deepDungeonEnd = true;
            // }
            return;
        }

        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Run finished pause!"), new UIForegroundPayload(0)));
        LogHelper.ReportSuccess("Run finished!");
        // ConnectionManager.onRun = false;
        // ConnectionManager.PauseQueueEnd();
    }

    private void OnDutyStarted(object? sender, ushort e)
    {
        if (!Dalamud.Conditions[ConditionFlag.InDeepDungeon]) return;

        // ConnectionManager.onRun = true;
        // ConnectionManager.TryReset();
        // if (!DeepDungeonHelper.IsStartingFloors())
        // {
        //     ConnectionManager.Resume();
        // }
        // else
        // {
        //     ConnectionManager.StartOrSplit();
        // }
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Started timer!"), new UIForegroundPayload(0)));
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