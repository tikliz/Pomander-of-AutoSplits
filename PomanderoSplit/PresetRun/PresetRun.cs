using System.Collections.Generic;
using System.IO;
using PomanderoSplit.RunHandler;
using PomanderoSplit.RunHandler.triggers;

namespace PomanderoSplit.PresetRuns;

public class PresetRun
{
    internal string FilePath { get; set; }
    public string FileName { get; set; }
    public GenericRun? GenericRun { get; set; }

    public PresetRun(string filePath, GenericRun? genericRun, string runName = "")
    {
        FilePath = filePath;
        GenericRun = genericRun;
        if (genericRun != null)
        {
            FileName = genericRun.Name;
        }
        else
        {
            FileName = runName;
        }
    }
}