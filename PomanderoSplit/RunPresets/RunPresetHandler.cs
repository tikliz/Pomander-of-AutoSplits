using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dalamud.Game.ClientState.Conditions;
using Newtonsoft.Json;
using PomanderoSplit.RunHandler;
using PomanderoSplit.RunHandler.triggers;

namespace PomanderoSplit.RunPresets;

public class RunPresetHandler
{
    public List<RunPreset> Presets = [];
    private string defaultPath = Dalamud.PluginInterface.ConfigDirectory.FullName + @"\run_presets\";
    private string includedFilesPath() => Path.Combine(Dalamud.PluginInterface.AssemblyLocation.Directory?.FullName!, @"RunPresets");
    private DirectoryInfo dir;

    private readonly object dirLock = new();

    public RunPresetHandler()
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
                    //
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

                    RunPreset runPreset = new RunPreset(path, genericRun);
                    Presets.Add(runPreset);
                }
            }
        }
    }

    public void Save(RunPreset preset)
    {
        lock (dirLock)
        {
            string jsonString = JsonConvert.SerializeObject(preset, Formatting.Indented, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            File.WriteAllText(preset.FilePath, jsonString);
            Dalamud.Chat.Print($"Saved preset on {preset.FilePath}");
        }
    }

    internal unsafe GenericRun? ReadGenericRunFile(FileInfo file)
    {
        lock (dirLock)
        {
            string jsonString = File.ReadAllText(file.FullName);
            try
            {
                return JsonConvert.DeserializeObject<GenericRun>(jsonString, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
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

}