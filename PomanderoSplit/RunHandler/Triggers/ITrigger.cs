using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dalamud.Game.ClientState.Conditions;

namespace PomanderoSplit.RunHandler.triggers;

public static class AllTriggers
{
    public static Type[] Get()
    {
        List<Type> triggers = [];
        triggers.Add(typeof(TriggerOnConditionChange));
        triggers.Add(typeof(TriggerOnDutyStarted));
        triggers.Add(typeof(TriggerOnDutyWiped));
        triggers.Add(typeof(TriggerOnDutyComplete));
        triggers.Add(typeof(TriggerDeepDungeonFail));
        return triggers.ToArray();
    }


}

/// <summary>
/// this class represent an trigger.
/// </summary>

[JsonPolymorphic(
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(TriggerOnConditionChange), nameof(TriggerOnConditionChange))]
[JsonDerivedType(typeof(TriggerOnDutyStarted), nameof(TriggerOnDutyStarted))]
[JsonDerivedType(typeof(TriggerOnDutyWiped), nameof(TriggerOnDutyWiped))]
[JsonDerivedType(typeof(TriggerOnDutyComplete), nameof(TriggerOnDutyComplete))]
[JsonDerivedType(typeof(TriggerDeepDungeonFail), nameof(TriggerDeepDungeonFail))]
public interface ITrigger : IDisposable
{
    /// <summary>
    /// This function is called when the trigger needs to be activated.
    /// </summary>
    /// <param name="finisher">Save and call the finisher when your event get trigered the bool disposes this trigger</param>
    public void Activate(Action<bool> finisher);

    public List<(ConditionFlag, bool)>? GetConditions() { return null; }
    public void SetConditions(List<(ConditionFlag, bool)> flags) { }

    public string GetTypeName()
    {
        return this.GetType().Name;
    }
    
}

/*

public class TriggerTemplate : ITrigger
{
    public Action<bool> Finisher { get; set; } = (_) => { };
    private bool Activated { get; set; } = false;

    [JsonIgnore]
    public void Activate(Action<bool> finisher)
    {
        Finisher = finisher;
        Activated = true;

        // register to any event here
    }

    private void OnEvent()
    {
        // your logic here
        
        Finisher.Invoke(true);
    }

    public void Dispose()
    {
        if (!Activated) return;
        
        // Dispose any event or resource initializated here

    }
}

*/