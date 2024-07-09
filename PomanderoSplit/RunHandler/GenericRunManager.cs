using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Conditions;
using PomanderoSplit.Utils;

namespace PomanderoSplit.RunHandler;

public class GenericRunManager
{
    private Plugin Plugin { get; init; }

    public List<GenericRun> Runs { get; private set; }

    private bool ShouldReset { get; set; } = true;

    public GenericRun? CurrentRun { get; private set; }

    public GenericRunManager(Plugin plugin)
    {
        Runs = [GenericRun.DeepDungeonRun()];
        Plugin = plugin;
        CurrentRun = Runs.LastOrDefault();

        Dalamud.ClientState.TerritoryChanged += OnTerritoryChanged;
        Dalamud.Conditions.ConditionChange += OnConditionChange;
        Dalamud.Duty.DutyWiped += OnDutyWiped;
        Dalamud.Duty.DutyCompleted += OnDutyCompleted;
        Dalamud.Duty.DutyStarted += OnDutyStarted;
    }

    public void MatchTrigger(TriggerType shotTrigger)
    {

        Trigger? trigger = CurrentRun?.CurrentSplit?.Objective.Triggers.Find(x => x.Type.Equals(shotTrigger));
        if (trigger != null)
        {
            trigger.Action(this);
        }

        RequiredTrigger? requiredTrigger = CurrentRun?.CurrentSplit?.Objective.RequiredSplitTrigger;
        if (requiredTrigger != null && requiredTrigger.Type.Equals(shotTrigger))
        {
            requiredTrigger.Action(this);
        }
    }

    public void Begin()
    {
        Dalamud.Chat.Print("BLABLA");
    }

    public void BeginOrResume()
    {
        if (CurrentRun != null && CurrentRun.BeginOrResume())
        {
            Plugin.ConnectionManager.StartOrSplit();

            Dalamud.Chat.Print("Starting run!");
        }
        else
        {
            Plugin.ConnectionManager.Resume();
            Dalamud.Chat.Print("Resuming run!");
        }
    }
    internal void SplitAndResume()
    {
        if (CurrentRun != null && CurrentRun.Split())
        {
            Plugin.ConnectionManager.Split();
            CurrentRun.Resume();
            Plugin.ConnectionManager.Resume();
            Dalamud.Chat.Print("Splitting and resuming!");
        }
    }
    public void Split()
    {
        if (CurrentRun != null && CurrentRun.Split())
        {
            // temporary af
            if (CurrentRun.IsPaused)
            {
                Plugin.ConnectionManager.Resume();
            }
            //

            Plugin.ConnectionManager.Split();


            // temporary
            if (CurrentRun.IsPaused)
            {
                Plugin.ConnectionManager.Pause();
            }
            //
            Dalamud.Chat.Print("Splitting!");
        }
    }

    public void StartOrSplit()
    {
        if (CurrentRun != null)
        {
            var result = CurrentRun.StartOrSplit();
            if (result != 0)
            {
                if (!CurrentRun.IsPaused)
                {
                    Plugin.ConnectionManager.StartOrSplit();
                    Dalamud.Chat.Print("Sent startorsplit!");
                }
                else
                {
                    CurrentRun.Resume();
                    Plugin.ConnectionManager.Resume();
                    Dalamud.Chat.Print("Resuming!");
                }
            }
        }
    }

    public void Pause()
    {
        if (CurrentRun != null && !CurrentRun.Pause())
        {
            Plugin.ConnectionManager.Pause();
            Dalamud.Chat.Print("Pausing!");
        }
    }

    public void NewRun()
    {
        Runs.Add(GenericRun.DeepDungeonRun());
    }

    public void TryReset()
    {
        if (!ShouldReset) return;
        ShouldReset = false;
        Plugin.ConnectionManager.Reset();
    }

    public void PauseQueueEnd()
    {
        ShouldReset = true;
        Plugin.ConnectionManager.Pause();
    }



    // SUBSCRIBERS

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
#if DEBUG
        if (flag == ConditionFlag.Occupied && value) Dalamud.Log.Debug($"{flag} : {value}");
#endif

        // test stuff
        if (flag == ConditionFlag.Mounted)
        {
            if (value)
            {
                MatchTrigger(TriggerType.LoadingHideUI);
            }
            else
            {
                MatchTrigger(TriggerType.MovedBetweenAreas);
            }

        }
        if (flag == ConditionFlag.InCombat && value)
        {
            MatchTrigger(TriggerType.DutyStarted);
            MatchTrigger(TriggerType.GetControl);
        }
        // ------

        // loading screen
        if (flag == ConditionFlag.Occupied33 && !Dalamud.Conditions[ConditionFlag.Unconscious])
        {
            if (value)
            {
                MatchTrigger(TriggerType.LoadingHideUI);
            }
                
        }

        // travelling between areas
        if (flag == ConditionFlag.BetweenAreas)
        {
            if (value)
            {
                MatchTrigger(TriggerType.MovingBetweenAreas);
            }
            else
            {
                MatchTrigger(TriggerType.MovedBetweenAreas);
            }
        }

        // deep dungeon death
        if (flag == ConditionFlag.Unconscious && value && Dalamud.Conditions[ConditionFlag.Occupied33])
        {
            MatchTrigger(TriggerType.ObjectiveFailed);
        }



        // if (!CurrentRun.Active) return;
        // if (flag == ConditionFlag.InDeepDungeon && !value) Plugin.ConnectionManager.Pause();
        // if (Dalamud.Conditions[ConditionFlag.InDeepDungeon])
        // {
        //     if (flag == ConditionFlag.Occupied33 && value && !Dalamud.Conditions[ConditionFlag.Unconscious])
        //     {
        //         Plugin.ConnectionManager.Pause();
        //         Dalamud.Log.Info("Paused loading maybe.");
        //     }

        //     // deep dungeon death
        //     if (flag == ConditionFlag.Unconscious && value && Dalamud.Conditions[ConditionFlag.Occupied33])
        //     {
        //         PauseQueueEnd();
        //         Dalamud.Log.Info($"OnConditionChange: Run ended {Helpers.GetCurrentFloor()}");
        //         Dalamud.Log.Info($"{Helpers.DeepDungeonData()}");
        //     }
        //     if (flag == ConditionFlag.BetweenAreas && !value && Dalamud.Conditions[ConditionFlag.InDeepDungeon] && !Helpers.IsFirstSetFloor())
        //     {
        //         Plugin.ConnectionManager.Resume();
        //         Plugin.ConnectionManager.Split();
        //         Dalamud.Log.Info($"OnConditionChange: Changed floors {Helpers.GetCurrentFloor()}");
        //         Dalamud.Log.Info($"{Helpers.DeepDungeonData()}");
        //     }
        // }
    }

    private void OnTerritoryChanged(ushort id)
    {
        // if (!ShouldReset) TryReset();
    }

    private void OnDutyWiped(object? _, ushort __)
    {
        // CurrentRun.Finish();
        // PauseQueueEnd();
    }

    private void OnDutyCompleted(object? _, ushort __)
    {
        // if (Dalamud.Conditions[ConditionFlag.InDeepDungeon])
        // {
        //     if (Helpers.GetCurrentFloor() == 100 || Helpers.GetCurrentFloor() == 200)
        //     {
        //         Plugin.ConnectionManager.Split();
        //         PauseQueueEnd();
        //         CurrentRun.Finish();
        //     }
        //     else
        //     {
        //         Plugin.ConnectionManager.Split();
        //         Plugin.ConnectionManager.Pause();
        //         ShouldReset = true;
        //     }
        //     return;
        // }

        // CurrentRun.Finish(false);
        // PauseQueueEnd();
    }

    private void OnDutyStarted(object? sender, ushort e)
    {
        MatchTrigger(TriggerType.DutyStarted);
        // if (!Dalamud.Conditions[ConditionFlag.InDeepDungeon]) return;

        // ShouldReset = true;
        // TryReset();
        // if (!Helpers.IsStartingFloors())
        // {
        //     Plugin.ConnectionManager.Resume();
        // }
        // else
        // {
        //     Plugin.ConnectionManager.StartOrSplit();
        // }
    }



    public void Dispose()
    {
        Dalamud.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Dalamud.Conditions.ConditionChange -= OnConditionChange;
        Dalamud.Duty.DutyWiped -= OnDutyWiped;
        Dalamud.Duty.DutyCompleted -= OnDutyCompleted;
        Dalamud.Duty.DutyStarted -= OnDutyStarted;
    }

}
