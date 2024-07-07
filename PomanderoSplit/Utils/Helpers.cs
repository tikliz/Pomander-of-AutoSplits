using ImGuiNET;

namespace PomanderoSplit.Utils;

public static class Helpers
{
    public static void RightAlign(float offset = 40.0f)
    {
        var windowSize = ImGui.GetWindowSize();
        var labelSize = ImGui.CalcTextSize("");
        var buttonPosX = windowSize.X - labelSize.X - offset;

        ImGui.SetCursorPosX(buttonPosX);
    }
}