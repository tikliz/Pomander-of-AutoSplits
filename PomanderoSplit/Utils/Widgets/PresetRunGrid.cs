using System;
using System.IO;
using System.Numerics;
using ImGuiNET;
using PomanderoSplit.Connection;
using PomanderoSplit.PresetRuns;

namespace PomanderoSplit.Utils;

public class PresetRunGrid
{
    private const int ROWSIZE = 3;
    public static (int row, int col)? selectedItem = null;
    public static string inputTextNameBuffer = "";

    private static uint ColorRed = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 0.2f));
    private static uint ColorWhite = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 0.2f));
    private static uint ColorTransparent = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 0.0f));
    private static float BorderThickness = 1.0f;
    public static void Draw(Plugin plugin)
    {
        // ImFont selectableFont = new();
        plugin.FileDialogManager.Draw();

        ImGui.BeginGroup();
        ImGui.BeginChild("PresetRunGrid", new Vector2(200, 0), true, ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoResize);

        int itemCount = plugin.PresetRunHandler.Presets.Count;
        int itemIdx = 0;

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        // Get the item rectangle
        Vector2 min;
        Vector2 max;

        uint borderColor = ColorWhite;
        while (itemIdx < itemCount)
        {
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
            // WIP
            // make font smaller for selectables
            // float tempFont = ImGui.GetFont().Scale;
            // ImGui.GetFont().Scale -= 0.3f;
            // ImGui.PushFont(ImGui.GetFont());

            Vector2 prevCursorPos = ImGui.GetCursorPos();
            ImGui.BeginGroup();
            if (ImGui.Selectable($"##selectable_{itemIdx}", isSelected, ImGuiSelectableFlags.None, new Vector2(47, 47)) && !isSelected)
            {
                // if (selectedItem != null && selectedItem.Value.runPreset.GenericRun != null)
                // {
                //     selectedItem.Value.runPreset.GenericRun.Dispose();
                // }
                // selectedItem = (row, col, preset);  // Select the new item
                plugin.PresetRunHandler.SetSelectedPreset(itemIdx);
                selectedItem = (row, col);

                inputTextNameBuffer = plugin.PresetRunHandler.SelectedPreset?.GenericRun?.Name == null ? "" : plugin.PresetRunHandler.SelectedPreset.GenericRun.Name;

            }
            if (itemIdx >= plugin.PresetRunHandler.Presets.Count)
            {
                ImGui.PopStyleVar();
                ImGui.PopID();
                break;
            }
            Vector2 afterCursorPos = ImGui.GetCursorPos();

            min = ImGui.GetItemRectMin();
            max = ImGui.GetItemRectMax();

            borderColor = ColorWhite; // White color

            // uint borderColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 0.2f)); // White color
            // float borderThickness = 1.0f;

            drawList.AddRect(min, max, borderColor, 0.0f, ImDrawFlags.None, BorderThickness);

            // Render the wrapped text manually

            ImGui.SetCursorPos(prevCursorPos);
            ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + 47);
            ImGui.TextWrapped(plugin.PresetRunHandler.Presets[itemIdx].RunName);
            ImGui.PopTextWrapPos();
            ImGui.SetCursorPos(afterCursorPos);
            ImGui.EndGroup();


            // ImGui.GetFont().Scale = tempFont;
            // ImGui.PopFont();
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
        ImGui.PushStyleColor(ImGuiCol.Button, ColorTransparent);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorTransparent);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorTransparent);
        if (ImGui.Button("+", new Vector2(50, 50)))
        {
            Action<bool, string> CreateNewPreset = (success, path) =>
            {
                if (success)
                {
#if DEBUG
                    Dalamud.Log.Debug($"PresetRunGrid, Trying to create {path}");
#endif

                    File.Create(path);
                    plugin.PresetRunHandler.UpdatePresets();
                }
            };
            // plugin.FileDialogManager.SaveFileDialog("RunPresets", "*.json", "newPreset", ".json", teste);
            plugin.FileDialogManager.SaveFileDialog("title", ".json", "newPreset", ".json", CreateNewPreset, $@"{Dalamud.PluginInterface.ConfigDirectory.FullName}/run_presets");
        }
        if (ImGui.IsItemHovered())
        {
            min = ImGui.GetItemRectMin();
            max = ImGui.GetItemRectMax();
            Vector2 center = new(((min.X + max.X) / 2) - 1, (min.Y + max.Y) / 2);
            drawList.AddCircleFilled(center, 15, ColorRed);
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
            drawList.AddCircle(center, 15, ColorWhite);
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