using System.Data;
using System.Text.Json;
using ImGuiNET;
using PomanderoSplit.RunHandler;

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
        string jsonString = JsonSerializer.Serialize(obj, options);
        Objective? temp = JsonSerializer.Deserialize<Objective>(jsonString, options); 
        if (temp == null)
        {
            throw new NoNullAllowedException();
        }
        return temp;
    }

    public static T LazyDeepCopy<T>(T obj)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
        };
        string jsonString = JsonSerializer.Serialize(obj, options);
        var temp = JsonSerializer.Deserialize<T>(jsonString);
        if (temp == null)
        {
            throw new NoNullAllowedException();
        }
        return temp;
    }
}