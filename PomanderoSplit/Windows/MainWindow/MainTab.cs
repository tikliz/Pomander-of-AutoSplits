using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using PomanderoSplit.RunHandler;
using PomanderoSplit.RunHandler.triggers;
using PomanderoSplit.PresetRuns;
using PomanderoSplit.Utils;

namespace PomanderoSplit.Windows;

public partial class MainWindow : Window, IDisposable
{
    // stuff for splitsconfig widget
    private int selectedTrigger = -1;
    bool popUpOpen = false;
    int indexObjective = 0;

    public void DrawMainTab()
    {
        using var tab = ImRaii.TabItem("Main");
        if (!tab) return;

        ImGui.Spacing();
        ImGui.Spacing();
        PresetRunGrid.Draw(Plugin);
        ImGui.SameLine();


        // ImGui.Separator();
        // ImGui.Spacing();
        ImGui.BeginGroup();
        ImGui.BeginChild("RunConfigStuff", new Vector2(180, 0), true, ImGuiWindowFlags.NoResize);

        ImGui.BeginGroup();
        if (Plugin.PresetRunHandler.SelectedPreset != null)
        {
            ImGui.InputText("##run_name", ref PresetRunGrid.inputTextNameBuffer, 64);
        }
        else
        {
            ImGui.InputText("##run_name", ref PresetRunGrid.inputTextNameBuffer, 64);
        }
        ImGui.SameLine();
        ImGui.BeginDisabled(
            PresetRunGrid.inputTextNameBuffer.Length <= 0
            || Plugin.PresetRunHandler.SelectedPreset == null
            || (Plugin.PresetRunHandler.SelectedPreset.GenericRun != null
                && PresetRunGrid.inputTextNameBuffer == Plugin.PresetRunHandler.SelectedPreset.GenericRun.Name)
            );
        if (ImGui.Button("Save##save_run_name"))
        {
            if (Plugin.PresetRunHandler.SelectedPreset?.GenericRun != null)
            {
                Plugin.PresetRunHandler.SelectedPreset.GenericRun.Name = PresetRunGrid.inputTextNameBuffer;
            }
        }
        ImGui.EndDisabled();
        ImGui.BeginChild("TriggerList", new Vector2(150, 60), true, ImGuiWindowFlags.AlwaysAutoResize);
        int count = 0;
        if (Plugin.PresetRunHandler.SelectedPreset != null)
        {
            if (ImGui.Selectable($"Start Run Triggers", selectedTrigger == 0))
            {
                if (selectedTrigger == 0)
                {
                    selectedTrigger = -1;
                }
                else
                {
                    ImGui.OpenPopup("beginTriggers_popup");
                }
            }

            if (ImGui.Selectable($"Other Triggers", selectedTrigger == 1))
            {
                if (selectedTrigger == 1)
                {
                    selectedTrigger = -1;
                }
                else
                {
                    ImGui.OpenPopup("teste");
                }
            }
            // var propertyInfos = typeof(Objective).GetProperties().Where(x => x.PropertyType == typeof(ITrigger[]));
            // foreach (var triggerType in propertyInfos)
            {
                // if (ImGui.Selectable($"{triggerType.Name}##{count}", selectedTrigger == count))
                // {
                //     if (selectedTrigger == count)
                //     {
                //         // ImGui.CloseCurrentPopup();
                //         selectedTrigger = -1;
                //     }
                //     else
                //     {
                //         Dalamud.Chat.Print($"CLICKED {triggerType.Name}");
                //         if (triggerType.Name == "Begin")
                //         {
                //             ImGui.OpenPopup("beginTriggers_popup");
                //         }
                //         else
                //         {
                //             ImGui.OpenPopup("teste");
                //         }
                //         selectedTrigger = count;
                //     }
                // }
                count++;
            }
        }

        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 2);
        // ImGui.PushStyleColor(ImGuiCol.PopupBg, ImGui.ColorConvertFloat4ToU32(new(0, 0, 0, 0.9f)));
        if (ImGui.BeginPopup("beginTriggers_popup", ImGuiWindowFlags.AlwaysAutoResize))
        {
            //
            popUpOpen = true;
            ITrigger[]? triggers = Plugin.PresetRunHandler.SelectedPreset?.GenericRun?.BeginRunTriggers;
            if (triggers != null)
            {
                var clone = triggers.ToList();
                for (int i = 0; i < clone.Count; i++)
                {
                    ImGui.Text($"{clone[i].GetName()}");
                    var conditionList = clone[i].GetConditions();
                    if (ImGui.IsItemHovered() && conditionList != null)
                    {
                        // var conditionList = trigger.GetConditions();
                        string tool_tip = "";
                        foreach ((var cond, var val) in conditionList)
                        {
                            string temp = val ? "active" : "not active";
                            tool_tip += $"If {cond} was {temp}\n";
                        }
                        ImGui.SetTooltip(tool_tip);
                    }
                    ImGui.SameLine();
                    ImGui.SmallButton("Edit");
                    ImGui.SameLine();
                    if (ImGui.SmallButton(" - "))
                    {
                        clone.RemoveAt(i);
                    }
                }
                Plugin.PresetRunHandler.SelectedPreset?.GenericRun?.SetBeginTriggers(clone.ToArray());
            }
            ImGui.Separator();

            ImGui.SmallButton("Add");
            ImGui.SameLine();
            ImGui.SmallButton("RemoveAll");
            ImGui.EndPopup();
        }
        else if (popUpOpen && selectedTrigger == 0)
        {
            Dalamud.Chat.Print($"Closing {selectedTrigger}");
            popUpOpen = false;
            selectedTrigger = -1;
        }

        if (ImGui.BeginPopup("teste"))
        {
            popUpOpen = true;

            Objective[]? objectives = Plugin.PresetRunHandler.SelectedPreset?.GenericRun?.Objectives;
            Objective? currentObjective = objectives?[indexObjective];
            if (objectives != null)
            {
                var size = objectives.Length - 1;
                float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
                ImGui.Text(objectives[indexObjective].Name);
                ImGui.SameLine();
                ImGui.PushButtonRepeat(true);
                ImGui.BeginDisabled(indexObjective == 0);
                if (ImGui.ArrowButton("##splitselector_left", ImGuiDir.Left))
                {
                    if (indexObjective != 0) indexObjective--;
                }
                ImGui.EndDisabled();
                ImGui.SameLine(0.0f, spacing);
                ImGui.BeginDisabled(size == indexObjective || indexObjective >= size);
                if (ImGui.ArrowButton("##splitselector_right", ImGuiDir.Right))
                {
                    if (size != indexObjective && indexObjective < size) indexObjective++;
                }
                ImGui.EndDisabled();
                ImGui.PopButtonRepeat();

                ImGui.SmallButton("Add##splits");
                ImGui.SameLine();
                ImGui.SmallButton("Remove##splits");
                ImGui.SameLine();
                ImGui.SmallButton("RemoveAll##splits");
                ImGui.Separator();
                // WIP
                // change this into a array for this widget later
                var propertyInfos = typeof(Objective).GetProperties().Where(x => x.PropertyType == typeof(ITrigger[]) && x.Name != "Begin");
                foreach (var propertyInfo in propertyInfos)
                {
                    // ImGui.Text($"{propertyInfo.Name}");
                    var triggerList = propertyInfo.GetValue(currentObjective) as ITrigger[];


                    if (ImGui.TreeNode($"{propertyInfo.Name}"))
                    {
                        if (triggerList != null && triggerList.Length > 0)
                        {
                            var tempList = triggerList.ToList();
                            for (int i = 0; i < triggerList.Length; i++)
                            {
                                ImGui.Text($"{triggerList[i].GetName()}");
                                var conditionList = triggerList[i].GetConditions();
                                if (ImGui.IsItemHovered() && conditionList != null)
                                {
                                    // var conditionList = trigger.GetConditions();
                                    string tool_tip = "";
                                    foreach ((var cond, var val) in conditionList)
                                    {
                                        string temp = val ? "active" : "not active";
                                        tool_tip += $"If {cond} was {temp}\n";
                                    }
                                    ImGui.SetTooltip(tool_tip);
                                }
                                ImGui.SameLine();
                                ImGui.SmallButton("Edit");
                                ImGui.SameLine();
                                if (ImGui.SmallButton(" X "))
                                {
                                    tempList.RemoveAt(i);
                                    propertyInfo.SetValue(currentObjective, tempList.ToArray());
                                }
                            }
                        }
                        ImGui.SmallButton($"Add##{propertyInfo.Name}");
                        ImGui.SameLine();
                        ImGui.SmallButton("Remove all");
                        ImGui.Separator();
                        ImGui.TreePop();
                    }
                    // ImGui.Separator();
                }
            }
            ImGui.Separator();
            ImGui.SmallButton("RemoveAllTriggers##triggers");
            ImGui.EndPopup();
        }
        else if (popUpOpen && selectedTrigger == 1)
        {
            popUpOpen = false;
            selectedTrigger = -1;
        }
        // ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        ImGui.EndChild();
        ImGui.EndGroup();

        if (ImGui.Button("Reset Run"))
        {
            Plugin.GenericRunManager.ResetRun();
        }

        if (ImGui.Button("Create Run"))
        {
            Plugin.GenericRunManager.CreateTestRun();
        }

        if (ImGui.Button("Save Preset") && Plugin.PresetRunHandler.SelectedPreset != null)
        {
            Plugin.PresetRunHandler.Save(Plugin.PresetRunHandler.SelectedPreset);
        }
        ImGui.EndChild();
        ImGui.EndGroup();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

    }
}
