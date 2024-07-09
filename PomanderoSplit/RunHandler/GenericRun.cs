using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FFXIVClientStructs.FFXIV.Common.Lua;
using PomanderoSplit.Utils;

namespace PomanderoSplit.RunHandler;


public class Objective
{
    public RequiredTrigger RequiredSplitTrigger { get; set; }
    public List<Trigger> Triggers { get; set; }
    public Objective(RequiredTrigger requiredSplitTrigger, List<Trigger>? triggers = null)
    {
        RequiredSplitTrigger = requiredSplitTrigger;
        Triggers = triggers ?? new List<Trigger>();
    }
}

public class Split
{
    public Objective Objective { get; set; }
    public TimeSpan Time { get; set; } = TimeSpan.Zero;
    public bool Complete { get; set; } = false;
    public string Name { get; set; }

    public Split(Objective objective, TimeSpan? time = null, string name = "")
    {
        Objective = objective;
        if (time != null)
        {
            Time = (TimeSpan)time;
        }
        Name = name;

    }

    public void Toggle()
    {
        Complete = !Complete;
    }

}


public class GenericRun
{
    private Stopwatch RunStopwatch { get; set; } = new();

    public bool Active { get; private set; } = false;
    public bool Finished { get; private set; } = false;
    public bool? Completed { get; private set; }
    public bool IsPaused { get; private set; } = false;

    public List<Split> Splits { get; private set; }
    public Split? CurrentSplit { get; private set; }
    private int activeIdx = 0;

    public TimeSpan Duration => RunStopwatch.Elapsed;

    public GenericRun(int splitCount, List<Objective> objectives)
    {
        if (splitCount <= 0) throw new Exception("Invalid splitCount for run.");
        Splits = new List<Split>();
        for (int i = 0; i < splitCount; i++)
        {
            var objective = objectives[i];
            Splits.Add(new Split(objective));
        }
        CurrentSplit = Splits[0];
    }

    public static GenericRun DeepDungeonRun(int floors = 100)
    {
        List<Objective> objectivesList = new();

        Action<GenericRunManager> beginFun = (manager) => { manager.BeginOrResume(); };
        Action<GenericRunManager> progressFun = (manager) => { manager.StartOrSplit(); };
        Action<GenericRunManager> pauseFun = (manager) => { manager.Pause(); };

        Action<GenericRunManager> bossKilled = (manager) =>
        {
            manager.Split();
            manager.Pause();
            manager.CurrentRun.Active = false;
        };
        for (int i = 1; i <= floors; i++)
        {
            List<Trigger> triggers = new();
            RequiredTrigger requiredTrigger;
            if (i % 10 == 1)
            {
                Trigger beginTrigger = new(TriggerType.DutyStarted, beginFun);
                triggers.Add(beginTrigger);
                Trigger loadingTrigger = new(TriggerType.LoadingHideUI, pauseFun);
                triggers.Add(loadingTrigger);
                Trigger deathTrigger = new(TriggerType.ObjectiveFailed, pauseFun);
                triggers.Add(deathTrigger);

                requiredTrigger = new(TriggerType.MovingBetweenAreas);
            }
            else
            {
                Trigger loadingTrigger = new(TriggerType.LoadingHideUI, pauseFun);
                triggers.Add(loadingTrigger);
                Trigger resumeTrigger = new(TriggerType.MovedBetweenAreas, beginFun);
                triggers.Add(resumeTrigger);
                Trigger deathTrigger = new(TriggerType.ObjectiveFailed, pauseFun);
                triggers.Add(deathTrigger);
                Trigger dutyComplete = new(TriggerType.DutyCompleted, bossKilled);
                triggers.Add(dutyComplete);

                requiredTrigger = new(TriggerType.MovingBetweenAreas);
            }

            objectivesList.Add(new(requiredTrigger, triggers));
        }
        GenericRun deepDungeonRun = new GenericRun(floors, objectivesList);
        return deepDungeonRun;
    }

    public void Begin()
    {
        if (Finished || Active || (Completed ?? false)) throw new Exception("Invalid Operation");

        RunStopwatch.Start();
        Active = true;

        // Splits.Add((Helpers.GetCurrentFloor(), TimeSpan.Zero));

        Dalamud.Log.Debug($"DeepDungeonRun Begin, {DateTime.Now}: Done");
    }

    public bool BeginOrResume()
    {
        if (Finished || (Completed ?? false)) throw new Exception("Invalid Operation");
        bool temp_active = Active;
        // 
        Dalamud.Chat.Print($"ACTIVE STATE: {Active}");
        RunStopwatch.Start();
        Active = true;
        IsPaused = false;

        // Splits.Add((Helpers.GetCurrentFloor(), TimeSpan.Zero));

        Dalamud.Log.Debug($"DeepDungeonRun Begin, {DateTime.Now}: Done");
        return !temp_active;
    }

    public void Finish(bool CompletedSucefully = false)
    {
        if (!Finished || !Active || !(Completed ?? false)) throw new Exception("Invalid Operation");
        RunStopwatch.Stop();
        Active = false;
        Finished = true;
        Completed = CompletedSucefully;

        // Splits.Add((Helpers.GetCurrentFloor(), Duration));

        Dalamud.Log.Debug($"DeepDungeonRun Finish, {DateTime.Now}: Done");
    }

    public bool Pause()
    {
        if (Finished || !Active) throw new Exception("Invalid Operation");
        RunStopwatch.Stop();
        IsPaused = true;

        Dalamud.Log.Debug($"DeepDungeonRun Pause, {DateTime.Now}: Done");
        return true;
    }

    public bool Resume()
    {
        if (!IsPaused || Finished || !Active) throw new Exception("Invalid Operation");
        RunStopwatch.Start();
        bool temp = IsPaused;
        IsPaused = false;

        Dalamud.Log.Debug($"DeepDungeonRun Continue, {DateTime.Now}: Done");
        return temp;
    }

    public bool Split()
    {
        if (Active != true) throw new Exception("Invalid Operation");
        // Splits.Add((Helpers.GetCurrentFloor(), Duration));

        if (CurrentSplit == null) throw new Exception("CurrentSplit is invalid");
        CurrentSplit?.Toggle();
        activeIdx++;
        bool oob = activeIdx < Splits.Count;
        if (oob)
        {
            CurrentSplit = Splits[activeIdx];
        }
        // else livesplit will pause, then reset, then start the timer from the beginning

        Dalamud.Log.Debug($"DeepDungeonRun Split, {DateTime.Now}: Done");
        return oob;
    }

    public int StartOrSplit()
    {
        if (Finished || !Active) throw new Exception("Invalid Operation");
        if (!Split()) return 0;

        RunStopwatch.Start();

        Dalamud.Log.Debug($"DeepDungeonRun Continue, {DateTime.Now}: Done");
        return IsPaused ? 1 : 2;
    }

}
