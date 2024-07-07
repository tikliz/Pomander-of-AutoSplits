using ImGuiNET;

namespace PomanderoSplit.Util;

public static partial class Widget
{
    public static void StatusCircle(bool status)
    {
        if (status)
        {
            ImGui.PushStyleColor(ImGuiCol.CheckMark, new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f)); // Green color
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.0f, 0.8f, 0.0f, 1.0f)); // Darker green when hovered
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.CheckMark, new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f)); // Red color
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.8f, 0.0f, 0.0f, 1.0f)); // Darker red when hovered
        }

        if (ImGui.RadioButton("", true))
        {
            // Button logic here (if any)
        }
        ImGui.PopStyleColor(2);

        if (ImGui.IsItemHovered())
        {
            if (status)
            {
                ImGui.SetTooltip("CONNECTED TO LIVESPLIT");
            }
            else
            {
                ImGui.SetTooltip("NOT CONNECTED TO LIVESPLIT");
            }
        }
    }

}