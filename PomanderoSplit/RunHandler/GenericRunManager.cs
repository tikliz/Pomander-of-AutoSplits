using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;

namespace PomanderoSplit.RunHandler;

public class GenericRunManager : IDisposable
{
    private Plugin Plugin { get; init; }

    public List<GenericRun> Runs { get; private set; }

    public GenericRun? CurrentRun() => Runs.FirstOrDefault();

    public GenericRunManager(Plugin plugin)
    {
        Plugin = plugin;
    }

    public void CreateRun()
    {
        if (Runs.Count == 0)
        {
            Runs = new();
        }
        var current = CurrentRun();

        if (current != null)
        {
            CurrentRun()?.Begin();
            CurrentRun()?.End(false);
        }

        GenericRun testRun = new GenericRun(
            $"teste {Runs.Count}",
            [
                new Objective()
                {
                    Name = "test Objective 1",
                    Split = new TriggerTest([(ConditionFlag.Mounted, true)], 1),
                    Pause = null,
                    Resume = null,
                    End = new TriggerEnd([(ConditionFlag.BetweenAreas, false)]),
                },
                new Objective()
                {
                    Name = "test Objective 2",
                    Split = new TriggerTest([(ConditionFlag.Mounted, true)], 2),
                    Pause = null,
                    Resume = null,
                    End = new TriggerEnd([(ConditionFlag.BetweenAreas, false)]),
                },
                new Objective()
                {
                    Name = "test Objective 3",
                    Split = null,
                    Pause = null,
                    Resume = null,
                    End = new TriggerTest([(ConditionFlag.Mounted, true)], 3, true),
                },
            ], new TriggerTest([(ConditionFlag.Mounted, false)], 0));

        Runs.Add(testRun);

        Dalamud.Log.Debug($"GenericRunManager CreateRun: Done -- run count: {Runs.Count}");
    }

    public void ResetRun()
    {
        var current = CurrentRun();
        if (current == null)
        {
            Dalamud.Log.Warning("GenericRunManager ResetRun: can not reset a non existing run");
            return;
        }

        Runs.Add(new(current.Name, current.Objectives, current.BeginRunTrigger));
        if (current.Status != RunState.Completed && current.Status != RunState.Failed) current.End(false);

        Dalamud.Log.Debug($"GenericRunManager ResetRun: Done");
    }

    public void Dispose()
    {
        foreach (var run in Runs) run.Dispose();
    }
}