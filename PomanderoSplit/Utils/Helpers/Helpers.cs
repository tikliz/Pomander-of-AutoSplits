using System.Data;
using System.Text.Json;
using ImGuiNET;

namespace PomanderoSplit.Utils;

public static partial class Helpers
{
    public static void RightAlign(float offset = 40.0f)
    {
        var windowSize = ImGui.GetWindowSize();
        var labelSize = ImGui.CalcTextSize("");
        var buttonPosX = windowSize.X - labelSize.X - offset;

        ImGui.SetCursorPosX(buttonPosX);
    }

    public static Objective LazyObjectiveDeepCopy<Objective>(Objective obj)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
        };
        var jsonString = JsonSerializer.Serialize(obj, options);
        var temp = JsonSerializer.Deserialize<Objective>(jsonString, options) ?? throw new NoNullAllowedException();
        return temp;
    }

    public static T LazyDeepCopy<T>(T obj)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
        };
        var jsonString = JsonSerializer.Serialize(obj, options);
        var temp = JsonSerializer.Deserialize<T>(jsonString) ?? throw new NoNullAllowedException();
        return temp;
    }
}