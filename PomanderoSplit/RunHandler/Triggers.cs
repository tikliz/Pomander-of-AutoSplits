using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace PomanderoSplit.RunHandler;

public enum TriggerType
{
    None,
    LoadingHideUI, // occupied33
    GetControl, // no longer occupied
    TerritoryChanged,
    Unconscious,
    DutyStarted,
    DutyWiped,
    DutyCompleted,
    ObjectiveComplete,
    ObjectiveFailed,
    MovingBetweenAreas,
    MovedBetweenAreas,
    InCombat,
    

}

public class Trigger
{
    public TriggerType Type { get; set;} = TriggerType.None;

    public Action<GenericRunManager> Action { get; set;} = (_) => { };
    public Trigger(TriggerType type = TriggerType.None, Action<GenericRunManager>? action = null)
    {

        Type = type;
        Action = action ?? ((_) => { });
    }

    public bool MatchTrigger(TriggerType shotTrigger)
    {
        return shotTrigger == this.Type;
    }
}

public class RequiredTrigger : Trigger
{
    public RequiredTrigger(TriggerType type)
    {
        Action<GenericRunManager> splitFun = (manager) => { manager.Split(); };
        Type = type;
        Action = splitFun;
    }
}