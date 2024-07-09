// public static GenericRun DeepDungeonRun(int floors = 100)
//     {
//         List<Objective> objectivesList = new();

//         Action<GenericRunManager> beginFun = (manager) => { manager.BeginOrResume(); };


//         Action<GenericRunManager> progressFun = (manager) => { manager.StartOrSplit(); };
//         Action<GenericRunManager> pauseFun = (manager) => { manager.Pause(); };

//         Action<GenericRunManager> bossKilled = (manager) =>
//         {
//             manager.Split();
//             manager.Pause();
//             manager.CurrentRun.Active = false;
//         };
//         for (int i = 1; i <= floors; i++)
//         {
//             List<Trigger> triggers = new();
//             RequiredTrigger requiredTrigger;
//             if (i % 10 == 1)
//             {
//                 Trigger beginTrigger = new(TriggerType.DutyStarted, beginFun);
//                 triggers.Add(beginTrigger);
//                 Trigger loadingTrigger = new(TriggerType.LoadingHideUI, pauseFun);
//                 triggers.Add(loadingTrigger);
//                 Trigger deathTrigger = new(TriggerType.ObjectiveFailed, pauseFun);
//                 triggers.Add(deathTrigger);

//                 requiredTrigger = new(TriggerType.MovingBetweenAreas);
//             }
//             else
//             {
//                 Trigger loadingTrigger = new(TriggerType.LoadingHideUI, pauseFun);
//                 triggers.Add(loadingTrigger);
//                 Trigger resumeTrigger = new(TriggerType.MovedBetweenAreas, beginFun);
//                 triggers.Add(resumeTrigger);
//                 Trigger deathTrigger = new(TriggerType.ObjectiveFailed, pauseFun);
//                 triggers.Add(deathTrigger);
//                 Trigger dutyComplete = new(TriggerType.DutyCompleted, bossKilled);
//                 triggers.Add(dutyComplete);

//                 requiredTrigger = new(TriggerType.MovingBetweenAreas);
//             }

//             objectivesList.Add(new(requiredTrigger, triggers));
//         }
//         GenericRun deepDungeonRun = new GenericRun(floors, objectivesList);
//         return deepDungeonRun;
//     }


    




    // public void Begin()
    // {
    //     Dalamud.Chat.Print("BLABLA");
    // }

    // public void BeginOrResume()
    // {
    //     if (CurrentRun != null && CurrentRun.BeginOrResume())
    //     {
    //         Plugin.ConnectionManager.Begin();

    //         Dalamud.Chat.Print("Starting run!");
    //     }
    //     else
    //     {
    //         Plugin.ConnectionManager.Resume();
    //         Dalamud.Chat.Print("Resuming run!");
    //     }
    // }
    // internal void SplitAndResume()
    // {
    //     if (CurrentRun != null && CurrentRun.Split())
    //     {
    //         Plugin.ConnectionManager.Split();
    //         CurrentRun.Resume();
    //         Plugin.ConnectionManager.Resume();
    //         Dalamud.Chat.Print("Splitting and resuming!");
    //     }
    // }
    // public void Split()
    // {
    //     if (CurrentRun != null && CurrentRun.Split())
    //     {
    //         // temporary af
    //         if (CurrentRun.IsPaused)
    //         {
    //             Plugin.ConnectionManager.Resume();
    //         }
    //         //

    //         Plugin.ConnectionManager.Split();


    //         // temporary
    //         if (CurrentRun.IsPaused)
    //         {
    //             Plugin.ConnectionManager.Pause();
    //         }
    //         //
    //         Dalamud.Chat.Print("Splitting!");
    //     }
    // }

    // public void StartOrSplit()
    // {
    //     if (CurrentRun != null)
    //     {
    //         var result = CurrentRun.StartOrSplit();
    //         if (result != 0)
    //         {
    //             if (!CurrentRun.IsPaused)
    //             {
    //                 Plugin.ConnectionManager.Begin();
    //                 Dalamud.Chat.Print("Sent startorsplit!");
    //             }
    //             else
    //             {
    //                 CurrentRun.Resume();
    //                 Plugin.ConnectionManager.Resume();
    //                 Dalamud.Chat.Print("Resuming!");
    //             }
    //         }
    //     }
    // }

    // public void Pause()
    // {
    //     if (CurrentRun != null && CurrentRun.Pause())
    //     {
    //         Plugin.ConnectionManager.Pause();
    //         Dalamud.Chat.Print("Pausing!");
    //     }
    // }

    // public void NewRun()
    // {
    //     Runs.Add(GenericRun.DeepDungeonRun());
    // }

    // public void TryReset()
    // {
    //     if (!ShouldReset) return;
    //     ShouldReset = false;
    //     Plugin.ConnectionManager.Reset();
    // }

    // public void PauseQueueEnd()
    // {
    //     ShouldReset = true;
    //     Plugin.ConnectionManager.Pause();
    // }



    

    // public void MatchTrigger(TriggerType shotTrigger)
    // {

    //     Trigger? trigger = CurrentRun?.CurrentSplit?.Objective.Triggers.Find(x => x.Type.Equals(shotTrigger));
    //     if (trigger != null)
    //     {
    //         trigger.Action(this);
    //     }

    //     RequiredTrigger? requiredTrigger = CurrentRun?.CurrentSplit?.Objective.RequiredSplitTrigger;
    //     if (requiredTrigger != null && requiredTrigger.Type.Equals(shotTrigger))
    //     {
    //         requiredTrigger.Action(this);
    //     }
    // }


// // SUBSCRIBERS

//     private void OnConditionChange(ConditionFlag flag, bool value)
//     {
// #if DEBUG
//         if (flag == ConditionFlag.Occupied && value) Dalamud.Log.Debug($"{flag} : {value}");
// #endif

//         // test stuff
//         if (flag == ConditionFlag.Mounted)
//         {
//             if (value)
//             {
//                 MatchTrigger(TriggerType.LoadingHideUI);
//             }
//             else
//             {
//                 MatchTrigger(TriggerType.MovedBetweenAreas);
//             }

//         }
//         if (flag == ConditionFlag.InCombat && value)
//         {
//             MatchTrigger(TriggerType.DutyStarted);
//             MatchTrigger(TriggerType.GetControl);
//         }
//         // ------

//         // loading screen
//         if (flag == ConditionFlag.Occupied33 && !Dalamud.Conditions[ConditionFlag.Unconscious])
//         {
//             if (value)
//             {
//                 MatchTrigger(TriggerType.LoadingHideUI);
//             }

//         }

//         // travelling between areas
//         if (flag == ConditionFlag.BetweenAreas)
//         {
//             if (value)
//             {
//                 MatchTrigger(TriggerType.MovingBetweenAreas);
//             }
//             else
//             {
//                 MatchTrigger(TriggerType.MovedBetweenAreas);
//             }
//         }

//         // deep dungeon death
//         if (flag == ConditionFlag.Unconscious && value && Dalamud.Conditions[ConditionFlag.Occupied33])
//         {
//             MatchTrigger(TriggerType.ObjectiveFailed);
//         }



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
    // }

    // private void OnTerritoryChanged(ushort id)
    // {
    //     // if (!ShouldReset) TryReset();
    // }

    // private void OnDutyWiped(object? _, ushort __)
    // {
    //     // CurrentRun.Finish();
    //     // PauseQueueEnd();
    // }

    // private void OnDutyCompleted(object? _, ushort __)
    // {
    //     MatchTrigger(TriggerType.DutyCompleted);
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
    // }

    // private void OnDutyStarted(object? sender, ushort e)
    // {
    //     MatchTrigger(TriggerType.DutyStarted);
    //     // if (!Dalamud.Conditions[ConditionFlag.InDeepDungeon]) return;

    //     // ShouldReset = true;
    //     // TryReset();
    //     // if (!Helpers.IsStartingFloors())
    //     // {
    //     //     Plugin.ConnectionManager.Resume();
    //     // }
    //     // else
    //     // {
    //     //     Plugin.ConnectionManager.StartOrSplit();
    //     // }
    // }



    // public void Dispose()
    // {
    //     Dalamud.ClientState.TerritoryChanged -= OnTerritoryChanged;
    //     Dalamud.Conditions.ConditionChange -= OnConditionChange;
    //     Dalamud.Duty.DutyWiped -= OnDutyWiped;
    //     Dalamud.Duty.DutyCompleted -= OnDutyCompleted;
    //     Dalamud.Duty.DutyStarted -= OnDutyStarted;
    // }
