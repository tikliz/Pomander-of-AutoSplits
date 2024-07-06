using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace PomanderoSplit.Widgets;

public static class StatusCircle
{
    public static void Draw()
    {
        if (Plugin.LiveSplitClient != null && Plugin.LiveSplitClient.livesplitSocket.Connected)
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
            if (Plugin.LiveSplitClient != null && Plugin.LiveSplitClient.livesplitSocket.Connected)
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