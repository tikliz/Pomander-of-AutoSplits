using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;

using PomanderoSplit.RunHandler.triggers;

namespace PomanderoSplit.RunHandler;

public class GenericRunManager : IDisposable
{
    private Plugin Plugin { get; init; }

    public List<GenericRun> Runs { get; private set; } = [];

    public GenericRun? CurrentRun() => Runs.LastOrDefault();

    public GenericRunManager(Plugin plugin)
    {
        Plugin = plugin;
    }

    public void CreateTestRun()
    {
        CurrentRun()?.Dispose();

        GenericRun testRun = new(
            $"teste {Runs.Count}",
            [
                new()
                {
                    Name = "test Objective 1",
                    Split = [new TriggerOnConditionChange([(ConditionFlag.Mounted, true)])],
                    End = [new TriggerOnConditionChange([(ConditionFlag.BetweenAreas, false)])],
                },
                new()
                {
                    Name = "test Objective 2",
                    Split = [new TriggerOnConditionChange([(ConditionFlag.Mounted, true)])],
                    End = [new TriggerOnConditionChange([(ConditionFlag.BetweenAreas, true)])],
                },
                new()
                {
                    Name = "test Objective 3",
                    End = [new TriggerOnConditionChange([(ConditionFlag.Mounted, true)])],
                },
            ],
            [new TriggerOnDutyWiped([(ConditionFlag.InDeepDungeon, false)])]
        );


        // example of how to use the events

        static void Onsplit(object? sender, EventArgs _)
        {
            var run = sender as GenericRun ?? throw new Exception("Invalid Sender");

            Dalamud.Chat.Print($"Name {run.Name}, On Split: {run.Splits.LastOrDefault()}");

            run.OnSplit -= Onsplit;
        }

        static void OnStatusChange(object? sender, EventArgs _)
        {
            var run = sender as GenericRun ?? throw new Exception("Invalid Sender");

            Dalamud.Chat.Print($"Name {run.Name}, On Status Change: {run.Status}");

            if (run.Status == RunState.Completed || run.Status == RunState.Failed)
            {
                run.OnStatusChange -= OnStatusChange;
                run.OnStatusChange -= Onsplit;
            };
        }

        testRun.OnSplit += Onsplit;
        testRun.OnStatusChange += OnStatusChange;



        Runs.Add(testRun);

        Dalamud.Log.Debug($"GenericRunManager CreateRun: Done {testRun.Name}");
    }

    public void CreateRunFromGenericRun(GenericRun genericRun)
    {
        CurrentRun()?.Dispose();

        GenericRun newRun = new GenericRun(genericRun.Name, genericRun.Objectives, genericRun.BeginRunTriggers, false);

        static void Onsplit(object? sender, EventArgs _)
        {
            var run = sender as GenericRun ?? throw new Exception("Invalid Sender");

            Dalamud.Chat.Print($"Name {run.Name}, On Split: {run.Splits.LastOrDefault()}");

            run.OnSplit -= Onsplit;
        }

        static void OnStatusChange(object? sender, EventArgs _)
        {
            var run = sender as GenericRun ?? throw new Exception("Invalid Sender");

            Dalamud.Chat.Print($"Name {run.Name}, On Status Change: {run.Status}");

            if (run.Status == RunState.Completed || run.Status == RunState.Failed)
            {
                run.OnStatusChange -= OnStatusChange;
                run.OnStatusChange -= Onsplit;
            };
        }

        newRun.OnSplit += Onsplit;
        newRun.OnStatusChange += OnStatusChange;



        Runs.Add(newRun);

        Dalamud.Log.Debug($"GenericRunManager CreateRun: Done {newRun.Name}");
    }

    public void ResetRun()
    {
        var current = CurrentRun();
        if (current == null)
        {
            Dalamud.Log.Warning("GenericRunManager ResetRun: can not reset a non existing run");
            return;
        }

        Runs.Add(new(current.Name, current.Objectives, current.BeginRunTriggers));
        if (current.Status != RunState.Completed && current.Status != RunState.Failed) current.End(false);

        Dalamud.Log.Debug($"GenericRunManager ResetRun: Done");
    }

    public void Dispose()
    {
        foreach (var run in Runs) run.Dispose();
    }
}