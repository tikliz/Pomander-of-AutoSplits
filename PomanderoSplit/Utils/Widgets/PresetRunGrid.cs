using System;
using System.Numerics;
using ImGuiNET;
using PomanderoSplit.Connection;
using PomanderoSplit.RunPresets;

namespace PomanderoSplit.Utils;

public class PresetRunGrid
{
    internal const int ROWSIZE = 3;
    public static (int row, int col, RunPreset? runPreset)? selectedItem = null;

    internal static uint colorRed = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 0.2f));
    internal static uint colorWhite = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 0.2f));
    internal static uint colorTransparent = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 0.0f));
    internal static float borderThickness = 1.0f;
    public static void Draw(Plugin plugin)
    {
        // ImFont selectableFont = new();

        ImGui.BeginGroup();
        ImGui.BeginChild("PresetRunGrid", new Vector2(200, 0), true, ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoResize);

        int itemCount = plugin.RunPresetHandler.Presets.Count;
        int itemIdx = 0;

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        // Get the item rectangle
        Vector2 min;
        Vector2 max;

        uint borderColor = colorWhite;
        while (itemIdx < itemCount)
        {
            RunPreset preset = plugin.RunPresetHandler.Presets[itemIdx];
            if (itemIdx % ROWSIZE != 0)
            {
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5.0f);
            }
            else
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5.0f);
            }
            (int row, int col) = GetRow(itemIdx);

            bool isSelected = selectedItem.HasValue && selectedItem.Value.row == row && selectedItem.Value.col == col;
            ImGui.PushID(row * 4 + col);
            //

            ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0, 1.0f));
            float tempFont = ImGui.GetFont().Scale;
            ImGui.GetFont().Scale -= 0.3f;
            ImGui.PushFont(ImGui.GetFont());

            Vector2 prevCursorPos = ImGui.GetCursorPos();
            ImGui.BeginGroup();
            if (ImGui.Selectable("##" + preset.GenericRun.Name, isSelected, ImGuiSelectableFlags.None, new Vector2(47, 47)))
            {
                selectedItem = (row, col, preset);  // Select the new item
            }
            Vector2 afterCursorPos = ImGui.GetCursorPos();

            min = ImGui.GetItemRectMin();
            max = ImGui.GetItemRectMax();

            borderColor = colorWhite; // White color

            // uint borderColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 0.2f)); // White color
            // float borderThickness = 1.0f;

            drawList.AddRect(min, max, borderColor, 0.0f, ImDrawFlags.None, borderThickness);

            // Render the wrapped text manually

            ImGui.SetCursorPos(prevCursorPos);
            ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + 47);
            ImGui.TextWrapped(preset.GenericRun.Name);
            ImGui.PopTextWrapPos();
            ImGui.SetCursorPos(afterCursorPos);
            ImGui.EndGroup();

            ImGui.GetFont().Scale = tempFont;
            ImGui.PopFont();
            ImGui.PopStyleVar();


            ImGui.PopID();
            itemIdx++;
        }
        if (itemIdx % ROWSIZE != 0)
        {
            ImGui.SameLine();
        }

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2);
        ImGui.PushStyleColor(ImGuiCol.Button, colorTransparent);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, colorTransparent);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, colorTransparent);
        bool buttonPressed = ImGui.Button("+", new Vector2(50, 50));
        if (ImGui.IsItemHovered())
        {
            min = ImGui.GetItemRectMin();
            max = ImGui.GetItemRectMax();
            Vector2 center = new(((min.X + max.X) / 2) - 1, (min.Y + max.Y) / 2);
            drawList.AddCircleFilled(center, 15, colorRed);
        }
        else
        {
            min = ImGui.GetItemRectMin();
            max = ImGui.GetItemRectMax();
            Vector2 center = new(((min.X + max.X) / 2) - 1, (min.Y + max.Y) / 2);
            drawList.AddCircleFilled(center, 15, borderColor);
        }
        if (ImGui.IsItemActive())
        {
            min = ImGui.GetItemRectMin();
            max = ImGui.GetItemRectMax();
            Vector2 center = new(((min.X + max.X) / 2) - 1, (min.Y + max.Y) / 2);
            drawList.AddCircle(center, 15, colorWhite);
        }
        if (buttonPressed)
        {
            // open new window to add preset
        }
        ImGui.PopStyleColor(3);
        // min = ImGui.GetItemRectMin();
        // max = ImGui.GetItemRectMax();
        // drawList.AddRect(min, max, borderColor, 0.0f, ImDrawFlags.None, borderThickness);
        ImGui.EndChild();
        ImGui.EndGroup();
    }

    private static (int row, int col) GetRow(int n)
    {
        int row = n / ROWSIZE;
        int col = n % ROWSIZE;
        return (row, col);
    }

}