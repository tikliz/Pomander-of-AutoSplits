using System;
using ImGuiNET;
using PomanderoSplit.Connection;

namespace PomanderoSplit.Utils;

public static partial class Widget
{
    public static void StatusCircle(ClientStatus status)
    {
        Action statusHandler = status switch
        {
            ClientStatus.Connected => () =>
            {
                ImGui.PushStyleColor(ImGuiCol.CheckMark, new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.0f, 0.8f, 0.0f, 1.0f));
            }
            ,
            ClientStatus.Disconnected => () =>
            {
                ImGui.PushStyleColor(ImGuiCol.CheckMark, new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.8f, 0.0f, 0.0f, 1.0f)); 
            }
            ,
            ClientStatus.Error => () =>
            {
                ImGui.PushStyleColor(ImGuiCol.CheckMark, new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.8f, 0.8f, 0.0f, 1.0f));
            }
            ,
            ClientStatus.NotInitialized => () =>
            {
                ImGui.PushStyleColor(ImGuiCol.CheckMark, new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.8f, 0.8f, 0.0f, 1.0f));
            }
            ,
            _ => throw new Exception("Status not handled"),
        };
        statusHandler();

        if (ImGui.RadioButton("", true))
        {
            // Button logic here (if any)
        }
        ImGui.PopStyleColor(2);

        if (ImGui.IsItemHovered())
        {
            Action tooltiphandler = status switch
            {
                ClientStatus.Connected => () =>
                {
                    ImGui.SetTooltip("Connected");
                }
                ,
                ClientStatus.Disconnected => () =>
                {
                    ImGui.SetTooltip("Disconnected");
                }
                ,
                ClientStatus.Error => () =>
                {
                    ImGui.SetTooltip("Error");
                }
                ,
                ClientStatus.NotInitialized => () =>
                {
                    ImGui.SetTooltip("NotInitialized");
                }
                ,
                _ => throw new Exception("Status not handled in tooltiphandler"),
            };
            tooltiphandler();
        }
    }

    public static void RightAlign(float offset = 40.0f)
    {
        var windowSize = ImGui.GetWindowSize();
        var labelSize = ImGui.CalcTextSize("");
        var buttonPosX = windowSize.X - labelSize.X - offset;

        ImGui.SetCursorPosX(buttonPosX);
    }
}