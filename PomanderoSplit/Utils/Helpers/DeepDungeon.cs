
using System.Text;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace PomanderoSplit.Utils;

public static unsafe partial class Helpers
{
    public static unsafe string DeepDungeonData()
    {
        InstanceContentDeepDungeon* deep_dungeon_data = GetInstanceContentDeepDungeon();

        if (deep_dungeon_data == null)
        {
            Dalamud.Log.Info("--- NULL DEEP DUNGEON DATA ---");
            return "empty";
        }

        StringBuilder sb = new();
        sb.Append($"Floor {deep_dungeon_data->Floor}");
        sb.AppendLine($"Recommended level? {deep_dungeon_data->GetRecommendedLevel()}");
        sb.AppendLine($"Passage Progress {deep_dungeon_data->PassageProgress}");
        sb.AppendLine($"Weapon level: {deep_dungeon_data->WeaponLevel}");
        sb.AppendLine($"Armor level: {deep_dungeon_data->ArmorLevel}");
        sb.AppendLine($"Content Flags: {deep_dungeon_data->ContentFlags}");

        return sb.ToString();
    }

    private static unsafe InstanceContentDeepDungeon* GetInstanceContentDeepDungeon()
    {
        var instance = EventFramework.Instance();
        return instance->GetInstanceContentDeepDungeon();
    }

    public static bool IsStartingFloors()
    {
        return GetCurrentFloor() == 1 || GetCurrentFloor() == 51;
    }

    public static bool IsFirstSetFloor()
    {
        return GetCurrentFloor() % 10 == 1;
    }

    public static int GetCurrentFloor()
    {
        var content = GetInstanceContentDeepDungeon();
        return content->Floor;
    }
}