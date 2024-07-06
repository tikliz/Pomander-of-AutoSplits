using ImGuiNET;

namespace PomanderoSplit.Widgets;

static class WidgetHelpers
{
    public static void RightAlign(float offset = 40.0f)
    {
        var windowSize = ImGui.GetWindowSize();
        var labelSize = ImGui.CalcTextSize("");
        float buttonPosX = windowSize.X - labelSize.X - offset;

        ImGui.SetCursorPosX(buttonPosX);
    }
}