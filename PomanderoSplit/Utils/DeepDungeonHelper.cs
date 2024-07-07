using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace PomanderoSplit.Utils;

public static unsafe class DeepDungeonHelper
{
    public static unsafe void LogDeepDungeonData()
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
        Dalamud.Log.Info($"Weapon level: {deep_dungeon_data->ContentFlags}");
    }

    public static unsafe InstanceContentDeepDungeon* GetInstanceContentDeepDungeon()
    {
        var instance = EventFramework.Instance();
        return instance->GetInstanceContentDeepDungeon();
    }

    public static void HandleConditions(ConditionFlag flag, bool value, LiveSplitClient LiveSplitClient)
    {
        if (flag == ConditionFlag.Occupied33 && value && !Dalamud.Conditions[ConditionFlag.Unconscious])
        {
            Dalamud.Chat.Print("Paused loading maybe.");
            LiveSplitClient.Pause();
        }
        if (flag == ConditionFlag.Unconscious && value && Dalamud.Conditions[ConditionFlag.Occupied33])
        {
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Run ended pausing."), new UIForegroundPayload(0)));
            LiveSplitClient.PauseQueueEnd();
            LogDeepDungeonData();
        }
        if (flag == ConditionFlag.BetweenAreas && !value && Dalamud.Conditions[ConditionFlag.InDeepDungeon] && !Is1thFloor())
        {
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Changed floors."), new UIForegroundPayload(0)));
            LiveSplitClient.Resume();
            LiveSplitClient.Split();
            LogDeepDungeonData();
        }
        
    }
    public static bool CheckRunFinished(LiveSplitClient LiveSplitClient)
    {
        if (Floor() == 100 || Floor() == 200)
        {
            LiveSplitClient.onRun = false;
            return true;
        }
        return false;
    }

    public static bool IsStartingFloors()
    {
        return Floor() == 1 || Floor() == 51;
    }
    private static bool Is1thFloor()
    {
        return Floor() % 10 == 1;
    }
    public static int Floor()
    {
        var content = GetInstanceContentDeepDungeon();
        return content->Floor;
    }

    private static void teste()
    {
        var content = GetInstanceContentDeepDungeon();
        var objective = content->Objectives;
    }
}