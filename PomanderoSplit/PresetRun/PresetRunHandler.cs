using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Dalamud.Game.ClientState.Conditions;
using Newtonsoft.Json.Linq;
using PomanderoSplit.RunHandler;
using PomanderoSplit.RunHandler.triggers;

namespace PomanderoSplit.PresetRuns;

public class PresetRunHandler
{
    public List<PresetRun> Presets = [];
    public PresetRun? SelectedPreset;
    private string defaultPath = Dalamud.PluginInterface.ConfigDirectory.FullName + @"\run_presets\";
    private string includedFilesPath() => Path.Combine(Dalamud.PluginInterface.AssemblyLocation.Directory?.FullName!, @"RunPresets");
    private DirectoryInfo dir;

    private readonly object dirLock = new();

    public PresetRunHandler()
    {
        lock (dirLock)
        {
            dir = new DirectoryInfo(defaultPath);
            if (!dir.Exists)
            {
                try
                {
#if DEBUG
                    Dalamud.Log.Debug($"RunPresetHandler, Trying to create preset folders in {dir} and copying included presets to it.");
#endif

                    dir.Create();
                    FileInfo[] includedFiles = new DirectoryInfo(includedFilesPath()).GetFiles();
                    var files = dir.GetFiles();
                    foreach (var file in includedFiles)
                    {
                        string newFile = Path.Combine(defaultPath + file.Name);
                        File.Copy(file.FullName, newFile, true);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            FileInfo[] fileInfo = dir.GetFiles("*.json");
            if (fileInfo.Length != 0)
            {
#if DEBUG
                Dalamud.Log.Debug($"RunPresetHandler, reading {fileInfo.Length} files from {dir}");
#endif
                foreach (FileInfo file in fileInfo)
                {
                    string name = file.Name;
                    string path = file.FullName;
                    GenericRun? genericRun = ReadGenericRunFile(file);
                    Dalamud.Log.Info($"{genericRun}");

                    if (genericRun == null)
                    {
                        genericRun = new(
                            ReadRunName(file),
                            [
                                new()
                            {
                                Name = "Objective 1",
                                Split = [new TriggerOnConditionChange([(ConditionFlag.Mounted, true), (ConditionFlag.AutorunActive, false)])],
                                End = [new TriggerEnd([(ConditionFlag.BetweenAreas, false)])],
                            },
                            new()
                            {
                                Name = "Objective 2",
                                Split = [new TriggerTest([(ConditionFlag.Mounted, true)], 2)],
                                End = [new TriggerEnd([(ConditionFlag.BetweenAreas, false)])],
                            },
                            new()
                            {
                                Name = "Objective 3",
                                End = [new TriggerTest([(ConditionFlag.Mounted, true)], 3, true)],
                            },
                            ],
                        [new TriggerTest([(ConditionFlag.Mounted, false)], 0)],
                        true
                        );
                    }

                    PresetRun runPreset = new PresetRun(path, null, name);
                    Presets.Add(runPreset);
                }
            }
        }
    }

    public void Save(PresetRun preset)
    {
        lock (dirLock)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
            };
            string jsonString = JsonSerializer.Serialize(preset, options);
            File.WriteAllText(preset.FilePath, jsonString);
            Dalamud.Chat.Print($"Saved preset on {preset.FilePath}");
        }
    }

    internal unsafe GenericRun? ReadGenericRunFile(FileInfo file)
    {
        lock (dirLock)
        {
#if DEBUG
            Dalamud.Log.Debug($"PresetRunHandler, trying to read {file.FullName}");
#endif
            string jsonString = File.ReadAllText(file.FullName);
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true,
                };
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    JsonElement root = document.RootElement.GetProperty("GenericRun");
                    string? name = root.GetProperty("Name").GetString();
                    if (name == null) return null;

                    JsonElement objectivesArray = root.GetProperty("Objectives");
                    Objective[]? objectives = JsonSerializer.Deserialize<Objective[]>(objectivesArray, options);
                    if (objectives == null) objectives = [];

                    JsonElement beginTriggersArray = root.GetProperty("BeginRunTriggers");
                    ITrigger[]? beginTriggers = JsonSerializer.Deserialize<ITrigger[]>(beginTriggersArray, options);
                    if (beginTriggers == null) beginTriggers = [];
                    // #if DEBUG
                    // Dalamud.Log.Debug($"READ {name} - {objectives.Length} objectives - {beginTriggers.Length} triggers");
                    // #endif
                    return new GenericRun(name, objectives, beginTriggers, true);
                }
            }
            catch (Exception e)
            {
                Dalamud.Log.Error($"RunPresetHandler, failed to parse file {file.FullName}: {e}");
                return null;
            }
        }
    }

    internal string ReadRunName(FileInfo file)
    {
        // WIP
        // read file contents and get run name
        string temp = file.Name.Split('.')[0];
        return temp;
    }

    public void SetSelectedPreset(int idx)
    {
        if (idx > Presets.Count) return;
        if (SelectedPreset != null && SelectedPreset.GenericRun != null)
        {
            Dalamud.Log.Debug($"Unloading {SelectedPreset.FileName}");
            SelectedPreset.GenericRun.Dispose();
            SelectedPreset.GenericRun = null;
        }
        if (Presets[idx].GenericRun == null)
        {
            LoadIntoRunPreset(idx);
        }
        SelectedPreset = Presets[idx];
    }

    public void LoadIntoRunPreset(int idx)
    {
        FileInfo file = new(Presets[idx].FilePath);
        GenericRun? genericRun = ReadGenericRunFile(file);
        string name = genericRun != null ? genericRun.Name : Presets[idx].FileName;
        var newRun = new PresetRun(Presets[idx].FilePath, genericRun, name);
        Presets[idx] = newRun;
        // runPreset.GenericRun = genericRun;
    }

}