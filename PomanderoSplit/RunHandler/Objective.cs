using System;

using PomanderoSplit.RunHandler.triggers;

namespace PomanderoSplit.RunHandler;

public class Objective : IDisposable
{
    public string Name { get; set; } = string.Empty;

    public ITrigger[]? Begin { get; set; } = null;
    public ITrigger[]? Split { get; set; } = null;
    public ITrigger[]? Pause { get; set; } = null;
    public ITrigger[]? Resume { get; set; } = null;
    public ITrigger[]? End { get; set; } = null;

    public void Init(GenericRun run)
    {
        static void ActivateTriggers(GenericRun run, string name, ITrigger[]? triggers, Action<bool> finisher)
        {
            if (triggers == null) return;

            foreach (var trigger in triggers)
            {
                try
                {
                    trigger.Activate((value) =>
                    {
                        try
                        {
                            finisher.Invoke(value);
                        }
                        catch (Exception ex)
                        {
                            Dalamud.Log.Error($"Objective Trigger {name} Finisher: {ex}");
                            run.Dispose();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dalamud.Log.Error($"Objective Trigger {name} Activation, index {Array.IndexOf(triggers, trigger)}: {ex}");
                }
            }
        }

        ActivateTriggers(run, "Begin", Begin, (dispose) => { run.Begin(); if (dispose) Dispose(); });
        ActivateTriggers(run, "Split", Split, (dispose) => { run.Split(); if (dispose) Dispose(); });
        ActivateTriggers(run, "Pause", Pause, (dispose) => { run.Pause(); if (dispose) Dispose(); });
        ActivateTriggers(run, "Resume", Resume, (dispose) => { run.Resume(); if (dispose) Dispose(); });
        ActivateTriggers(run, "End", End, (CompletedSuccessfully) => { run.End(CompletedSuccessfully); Dispose(); });
    }

    public override string ToString() => Name;

    public void Dispose()
    {
        if (Begin != null) foreach (var trigger in Begin) trigger.Dispose();
        if (Split != null) foreach (var trigger in Split) trigger.Dispose();
        if (Pause != null) foreach (var trigger in Pause) trigger.Dispose();
        if (Resume != null) foreach (var trigger in Resume) trigger.Dispose();
        if (End != null) foreach (var trigger in End) trigger.Dispose();
    }
}