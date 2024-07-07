
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using PomanderoSplit.Utils;

namespace PomanderoSplit;

public class EventSubscribers
{
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

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        // deep dungeon stuff
        if (LiveSplitClient.onRun)
        {
            if (flag == ConditionFlag.InDeepDungeon && !value)
            {
                Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Paused splits!"), new UIForegroundPayload(0)));
                LiveSplitClient.Pause();
            }
            if (Dalamud.Conditions[ConditionFlag.InDeepDungeon])
            {
                DeepDungeonHelper.HandleConditions(flag, value, LiveSplitClient);
            }
        }


        if (flag == ConditionFlag.Occupied && value)
        {
            Dalamud.Log.Info($"{flag} : {value}");
        }
    }

    private void OnTerritoryChanged(ushort id)
    {
        if (LiveSplitClient.resetSplits || !LiveSplitClient.deepDungeonEnd) LiveSplitClient.Reset();
    }

    private void OnDutyWiped(object? sender, ushort e)
    {
        Dalamud.Chat.Print("DUTY WIPED");
        // if (!Dalamud.Conditions[ConditionFlag.InDeepDungeon]) return;

        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload("Run ended pause!"), new UIForegroundPayload(0)));
        LiveSplitClient.onRun = false;
        LiveSplitClient.PauseQueueEnd();
    }

    private void OnDutyCompleted(object? sender, ushort e)
    {
        if (Dalamud.Conditions[ConditionFlag.InDeepDungeon])
        {
            if (DeepDungeonHelper.CheckRunFinished(LiveSplitClient))
            {
                Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Run finished pause!"), new UIForegroundPayload(0)));
                LogHelper.ReportSuccess("Run finished!");
                LiveSplitClient.Split();
                LiveSplitClient.PauseQueueEnd();
                LiveSplitClient.onRun = false;
            }
            else
            {
                Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Floorset finished pause!"), new UIForegroundPayload(0)));
                LogHelper.ReportSuccess("Floorset finished pause!");
                LiveSplitClient.Split();
                LiveSplitClient.Pause();
                LiveSplitClient.deepDungeonEnd = true;
            }
            return;
        }

        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Run finished pause!"), new UIForegroundPayload(0)));
        LogHelper.ReportSuccess("Run finished!");
        LiveSplitClient.onRun = false;
        LiveSplitClient.PauseQueueEnd();
    }

    private void OnDutyStarted(object? sender, ushort e)
    {
        if (!Dalamud.Conditions[ConditionFlag.InDeepDungeon]) return;

        LiveSplitClient.onRun = true;
        LiveSplitClient.TryReset();
        if (!DeepDungeonHelper.IsStartingFloors())
        {
            LiveSplitClient.Resume();
        }
        else
        {
            LiveSplitClient.StartOrSplit();
        }
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