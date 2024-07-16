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
using Dalamud.Game.ClientState.Conditions;

namespace PomanderoSplit.Windows;

public partial class MainWindow : Window, IDisposable
{
    // stuff for splitsconfig widget
    private int selectedTrigger = -1;
    bool popUpOpen = false;
    int indexObjective = 0;
    int splitsCopyBeginIdx = 0;
    int splitsCopyEndIdx = 0;
    string splitNameBuffer = "";

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
        ImGui.PushItemWidth(60);
        if (Plugin.PresetRunHandler.SelectedPreset != null)
        {
            ImGui.InputText("##run_name", ref PresetRunGrid.inputTextNameBuffer, 64);
        }
        else
        {
            ImGui.InputText("##run_name", ref PresetRunGrid.inputTextNameBuffer, 64);
        }
        ImGui.PopItemWidth();
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
                Plugin.PresetRunHandler.SelectedPreset.RunName = PresetRunGrid.inputTextNameBuffer;
            }
        }
        ImGui.EndDisabled();
        if (Plugin.PresetRunHandler.SelectedPreset?.GenericRun != null
                && PresetRunGrid.inputTextNameBuffer != Plugin.PresetRunHandler.SelectedPreset.GenericRun.Name)
        {
            ImGui.SameLine();
            if (ImGui.Button("Undo##undo_name_change"))
            {
                PresetRunGrid.inputTextNameBuffer = Plugin.PresetRunHandler.SelectedPreset.GenericRun.Name;
            }
        }
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
                    ImGui.OpenPopup("otherTriggers_popup");
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
                //             ImGui.OpenPopup("otherTriggers_popup");
                //         }
                //         selectedTrigger = count;
                //     }
                // }
                count++;
            }
        }

        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 2);
        // ImGui.PushStyleColor(ImGuiCol.PopupBg, ImGui.ColorConvertFloat4ToU32(new(0, 0, 0, 0.9f)));
        Vector2 pos = ImGui.GetWindowPos();
        Vector2 wSize = ImGui.GetWindowSize();
        pos.X += wSize.X + 30.0f;
        pos.Y -= wSize.Y + 20.0f;
        ImGui.SetNextWindowPos(pos);
        if (ImGui.BeginPopup("beginTriggers_popup", ImGuiWindowFlags.AlwaysAutoResize))
        {
            popUpOpen = true;
            ITrigger[]? triggers = Plugin.PresetRunHandler.SelectedPreset?.GenericRun?.BeginRunTriggers;
            if (triggers != null)
            {
                var clone = triggers.ToList();
                for (int i = 0; i < triggers.Length; i++)
                {
                    string comboPreview = triggers[i].GetTypeName();
                    ImGui.SetNextItemWidth(ImGui.CalcTextSize(comboPreview).X + ImGui.GetStyle().FramePadding.X + 5.0f);
                    if (ImGui.BeginCombo($"##triggerSelector_{i}", comboPreview, ImGuiComboFlags.NoArrowButton))
                    {
                        for (int n = 0; n < TriggerTypes.Length; n++)
                        {
                            bool is_selected = triggers[i].GetType() == TriggerTypes[n];
                            if (ImGui.Selectable(TriggerTypes[n].Name, is_selected))
                                clone[i] = (ITrigger)Activator.CreateInstance(TriggerTypes[n])!;

                            // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                            if (is_selected)
                                ImGui.SetItemDefaultFocus();
                        }
                        ImGui.EndCombo();
                    }
                    var conditionList = triggers[i].GetConditions();
                    if (ImGui.IsItemHovered())
                    {
                        // var conditionList = trigger.GetConditions();
                        string tool_tip = "";
                        if (conditionList == null || conditionList.Count == 0)
                        {
                            tool_tip = "ConditionFlags not set.";
                        }
                        else
                        {
                            foreach ((var cond, var val) in conditionList)
                            {
                                // WIP
                                // implement trigger tooltip
                                string temp = val ? "active" : "not active";
                                tool_tip += $"If {cond} was {temp}\n";
                            }
                        }
                        ImGui.SetTooltip(tool_tip);
                    }
                    if (clone[i].GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(x => x.Name == "Flags") != null)
                    {
                        // string comboPreview = triggers[0].GetTypeName();
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(ImGui.CalcTextSize("Flags").X + ImGui.GetStyle().FramePadding.X + 5.0f);
                        if (ImGui.BeginCombo($"##triggerFlagSelector_{i}", "Flags", ImGuiComboFlags.NoArrowButton))
                        {
                            List<(ConditionFlag, bool)> tempFlags = [];
                            bool update = false;
                            foreach (ConditionFlag flag in Enum.GetValues(typeof(ConditionFlag)))
                            {
                                if (flag == ConditionFlag.None) continue;
                                (ConditionFlag, bool)? cloneFlag = clone[i].GetConditions()?.FirstOrDefault(x => x.Item1 == flag);
                                bool is_selected = cloneFlag?.Item1 == flag;
                                // ImGui.SetItemAllowOverlap();
                                ImGui.Separator();
                                ImGui.Text($"{flag} :");
                                ImGui.SameLine();
                                // bool temp = false;
                                if (ImGui.Checkbox($"##{flag}", ref is_selected))
                                {
                                    update = true;
                                }
                                ImGui.SameLine();
                                ImGui.BeginDisabled(!is_selected);
                                ImGui.Text($"Trigger after");
                                ImGui.SameLine();
                                bool triggerOrder = cloneFlag == null ? false : cloneFlag.Value.Item2;
                                if (ImGui.Checkbox($"##{flag}_bool", ref triggerOrder))
                                {
                                    update = true;
                                }
                                ImGui.EndDisabled();
                                if (is_selected)
                                {
                                    tempFlags.Add((flag, triggerOrder));
                                }
                            }
                            if (update) clone[i].SetConditions(tempFlags);
                            ImGui.EndCombo();
                        }
                    }
                    ImGui.SameLine();
                    if (ImGui.SmallButton($" X ##beginTriggers_{triggers[i]}_{i}"))
                    {
                        clone.RemoveAt(i);
                    }
                }
                Plugin.PresetRunHandler.SelectedPreset?.GenericRun?.SetBeginTriggers(clone.ToArray());
            }
            ImGui.Separator();

            if (ImGui.SmallButton($"Add##beginTriggers"))
            {
                List<ITrigger> tempList = new();
                if (triggers != null)
                {
                    tempList = triggers.ToList();
                    tempList.Add(new TriggerOnConditionChange());
                    Plugin.PresetRunHandler.SelectedPreset?.GenericRun?.SetBeginTriggers(tempList.ToArray());
                }
            }
            ImGui.SameLine();
            if (ImGui.SmallButton($"Remove all##beginTriggers"))
            {
                Plugin.PresetRunHandler.SelectedPreset?.GenericRun?.SetBeginTriggers([]);
            }
            ImGui.EndPopup();
        }
        else if (popUpOpen && selectedTrigger == 0)
        {
            Dalamud.Chat.Print($"Closing {selectedTrigger}");
            popUpOpen = false;
            selectedTrigger = -1;
        }

        ImGui.SetNextWindowPos(pos);
        if (ImGui.BeginPopup("otherTriggers_popup"))
        {
            popUpOpen = true;

            Objective[]? objectives = Plugin.PresetRunHandler.SelectedPreset?.GenericRun?.Objectives;
            Objective? currentObjective = objectives?[indexObjective];
            if (objectives != null)
            {
                var size = objectives.Length - 1;
                float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
                splitNameBuffer = currentObjective == null ? "" : currentObjective.Name;
                ImGui.SetNextItemWidth(ImGui.CalcTextSize("Palace Of The Dead: Floor 200").X + ImGui.GetStyle().FramePadding.X + 5.0f);
                if (ImGui.InputText("##split_name", ref splitNameBuffer, 64, ImGuiInputTextFlags.None))
                {
                    objectives[indexObjective].Name = splitNameBuffer;

                }
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                ImGui.PushButtonRepeat(true);
                ImGui.BeginDisabled(indexObjective == 0);
                if (ImGui.ArrowButton("##splitselector_left", ImGuiDir.Left))
                {
                    if (indexObjective != 0) indexObjective--;
                    var objective_name = objectives[indexObjective]?.Name;
                    splitNameBuffer = objective_name == null ? "" : objective_name;
                }
                ImGui.EndDisabled();
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 3);
                ImGui.PushItemWidth(45);
                ImGui.DragInt("##splitselector_drag", ref indexObjective, 0.5f, 0, objectives.Length - 1, "%d", ImGuiSliderFlags.AlwaysClamp);
                ImGui.PopItemWidth();
                ImGui.SameLine(0.0f, spacing);
                ImGui.BeginDisabled(size == indexObjective || indexObjective >= size);
                if (ImGui.ArrowButton("##splitselector_right", ImGuiDir.Right))
                {
                    if (size != indexObjective && indexObjective < size) indexObjective++;
                    var objective_name = objectives?[indexObjective]?.Name;
                    splitNameBuffer = objective_name == null ? "" : objective_name;
                }
                ImGui.EndDisabled();
                ImGui.PopButtonRepeat();

                if (ImGui.SmallButton("Add##splits"))
                {
                    var tempObjectives = objectives!.ToList();
                    tempObjectives.Add(new Objective() { Name = "defaultName" });
                    indexObjective = tempObjectives.Count - 1;
                    Plugin.PresetRunHandler.SelectedPreset?.GenericRun?.SetObjectives(tempObjectives.ToArray());
                    
                }
                ImGui.SameLine();
                ImGui.BeginDisabled(objectives?.Length <= 1);
                if (ImGui.SmallButton("Remove##splits"))
                {
                    var tempObjectives = objectives!.ToList();
                    tempObjectives!.RemoveAt(indexObjective);
                    if (indexObjective == tempObjectives!.Count) indexObjective--;
                    Plugin.PresetRunHandler.SelectedPreset!.GenericRun!.SetObjectives(tempObjectives!.ToArray());
                }
                ImGui.EndDisabled();
                ImGui.SameLine();
                if (ImGui.SmallButton("RemoveAll##splits"))
                {
                    indexObjective = 0;
                    Plugin.PresetRunHandler.SelectedPreset?.GenericRun?.SetObjectives([new Objective() { Name = "defaultName" }]);
                }

                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10.0f);
                ImGui.BeginDisabled(splitsCopyEndIdx == splitsCopyBeginIdx || splitsCopyEndIdx > objectives?.Length);
                if (ImGui.Button("Copy##splits"))
                {
                    // WIP
                    // implement later actual deepcopy instead of this
                    var tempList = objectives!.ToList();
                    for (int i = splitsCopyBeginIdx; i < splitsCopyEndIdx; i++)
                    {
                        Objective temp = Helpers.LazyObjectiveDeepCopy(tempList[i]);
                        tempList.Add(temp);
                    }
                    Plugin.PresetRunHandler.SelectedPreset!.GenericRun!.SetObjectives(tempList.ToArray());
                }
                ImGui.EndDisabled();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip($"Copy splits from position [{splitsCopyBeginIdx}] to [{splitsCopyEndIdx}] and append them.\nClick and drag or double click to set the positions.");
                }
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 3);
                ImGui.PushItemWidth(35);
                ImGui.DragInt("##rangecopy_begin", ref splitsCopyBeginIdx, 1f, 0, splitsCopyEndIdx, "%d", ImGuiSliderFlags.AlwaysClamp);
                ImGui.PopItemWidth();
                
                
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 3);
                ImGui.PushItemWidth(35);
                ImGui.DragInt("##rangecopy_end", ref splitsCopyEndIdx, 1f, 0, objectives!.Length, "%d", ImGuiSliderFlags.AlwaysClamp);
                ImGui.PopItemWidth();
                
                ImGui.Separator();
                
                // WIP
                // change this into a array for this widget later
                var propertyInfos = typeof(Objective).GetProperties().Where(x => x.PropertyType == typeof(ITrigger[]) && x.Name != "Begin");
                foreach (var propertyInfo in propertyInfos)
                {
                    // ImGui.Text($"{propertyInfo.Name}");
                    ITrigger[]? triggers = propertyInfo.GetValue(currentObjective) as ITrigger[];
                    if (ImGui.TreeNode($"{propertyInfo.Name}"))
                    {
                        if (triggers != null && triggers.Length > 0)
                        {
                            var clone = triggers.ToList();
                            for (int i = 0; i < triggers.Length; i++)
                            {
                                string comboPreview = triggers[i].GetTypeName();
                                ImGui.SetNextItemWidth(ImGui.CalcTextSize(comboPreview).X + ImGui.GetStyle().FramePadding.X + 5.0f);
                                if (ImGui.BeginCombo($"##triggerSelector_{i}", comboPreview, ImGuiComboFlags.NoArrowButton))
                                {
                                    for (int n = 0; n < TriggerTypes.Length; n++)
                                    {
                                        bool is_selected = triggers[i].GetType() == TriggerTypes[n];
                                        if (ImGui.Selectable(TriggerTypes[n].Name, is_selected))
                                            clone[i] = (ITrigger)Activator.CreateInstance(TriggerTypes[n])!;

                                        // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                                        if (is_selected)
                                            ImGui.SetItemDefaultFocus();
                                    }
                                    ImGui.EndCombo();
                                }
                                var conditionList = triggers[i].GetConditions();
                                if (ImGui.IsItemHovered())
                                {
                                    // var conditionList = trigger.GetConditions();
                                    string tool_tip = "";
                                    if (conditionList == null || conditionList.Count == 0)
                                    {
                                        tool_tip = "ConditionFlags not set.";
                                    }
                                    else
                                    {
                                        foreach ((var cond, var val) in conditionList)
                                        {
                                            // WIP
                                            // implement trigger tooltip
                                            string temp = val ? "active" : "not active";
                                            tool_tip += $"If {cond} was {temp}\n";
                                        }
                                    }
                                    ImGui.SetTooltip(tool_tip);
                                }
                                ImGui.SameLine();
                                if (clone[i].GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(x => x.Name == "Flags") != null)
                                {
                                    // string comboPreview = triggers[0].GetTypeName();
                                    ImGui.SameLine();
                                    ImGui.SetNextItemWidth(ImGui.CalcTextSize("Flags").X + ImGui.GetStyle().FramePadding.X + 5.0f);
                                    if (ImGui.BeginCombo($"##triggerFlagSelector_{i}", "Flags", ImGuiComboFlags.NoArrowButton))
                                    {
                                        List<(ConditionFlag, bool)> tempFlags = [];
                                        bool update = false;
                                        foreach (ConditionFlag flag in Enum.GetValues(typeof(ConditionFlag)))
                                        {
                                            if (flag == ConditionFlag.None) continue;
                                            (ConditionFlag, bool)? cloneFlag = clone[i].GetConditions()?.FirstOrDefault(x => x.Item1 == flag);
                                            bool is_selected = cloneFlag?.Item1 == flag;
                                            // ImGui.SetItemAllowOverlap();
                                            ImGui.Separator();
                                            ImGui.Text($"{flag} :");
                                            ImGui.SameLine();
                                            // bool temp = false;
                                            if (ImGui.Checkbox($"##{flag}", ref is_selected))
                                            {
                                                update = true;
                                            }
                                            ImGui.SameLine();
                                            ImGui.BeginDisabled(!is_selected);
                                            ImGui.Text($"Trigger after");
                                            ImGui.SameLine();
                                            bool triggerOrder = cloneFlag == null ? false : cloneFlag.Value.Item2;
                                            if (ImGui.Checkbox($"##{flag}_bool", ref triggerOrder))
                                            {
                                                update = true;
                                            }
                                            ImGui.EndDisabled();
                                            if (is_selected)
                                            {
                                                tempFlags.Add((flag, triggerOrder));
                                            }
                                        }
                                        if (update) clone[i].SetConditions(tempFlags);
                                        ImGui.EndCombo();
                                    }
                                }
                                ImGui.SameLine();
                                if (ImGui.SmallButton($" X ##trigger_{propertyInfo.Name}_{i}"))
                                {
                                    clone.RemoveAt(i);
                                }
                            }
                            propertyInfo.SetValue(currentObjective, clone.ToArray());
                        }
                        if (ImGui.SmallButton($"Add##{propertyInfo.Name}"))
                        {
                            List<ITrigger> tempList = new();
                            if (triggers != null)
                            {
                                tempList = triggers.ToList();
                            }
                            tempList.Add(new TriggerOnConditionChange());
                            propertyInfo.SetValue(currentObjective, tempList.ToArray());
                        }
                        ImGui.SameLine();
                        if (ImGui.SmallButton($"Remove all##{propertyInfo.Name}"))
                        {
                            propertyInfo.SetValue(currentObjective, new ITrigger[0]);
                        }
                        ImGui.Separator();
                        ImGui.TreePop();
                    }
                    // ImGui.Separator();
                }
            }
            ImGui.Separator();
            if (ImGui.SmallButton("RemoveAllTriggers##triggers"))
            {
                var propertyInfos = typeof(Objective).GetProperties().Where(x => x.PropertyType == typeof(ITrigger[]));
                foreach (var propertyInfo in propertyInfos)
                {
                    propertyInfo.SetValue(currentObjective, new ITrigger[0]);
                }
            }
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

        if (ImGui.Button("Save Preset") && Plugin.PresetRunHandler.SelectedPreset != null)
        {
            Plugin.PresetRunHandler.Save(Plugin.PresetRunHandler.SelectedPreset);
        }
        if (ImGui.Button("Reset Run"))
        {
            Plugin.GenericRunManager.ResetRun();
        }

        ImGui.BeginDisabled(Plugin.PresetRunHandler.SelectedPreset?.GenericRun == null);
        if (ImGui.Button("Start Run"))
        {
            Plugin.GenericRunManager.CreateRunFromGenericRun(Plugin.PresetRunHandler.SelectedPreset?.GenericRun!);
        }
        ImGui.EndDisabled();

        ImGui.EndChild();
        ImGui.EndGroup();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

    }
}
