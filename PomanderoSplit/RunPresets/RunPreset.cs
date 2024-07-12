using System.Collections.Generic;
using System.IO;
using PomanderoSplit.RunHandler;
using PomanderoSplit.RunHandler.triggers;

namespace PomanderoSplit.RunPresets;

public class RunPreset
{
    internal string FilePath { get; set; }
    public GenericRun GenericRun { get; set; }

    public RunPreset(string filePath, GenericRun genericRun)
    {
        FilePath = filePath;
        GenericRun = genericRun;
    }
}