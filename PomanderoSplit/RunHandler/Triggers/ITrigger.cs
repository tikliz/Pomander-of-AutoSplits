using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dalamud.Game.ClientState.Conditions;

namespace PomanderoSplit.RunHandler.triggers;

/// <summary>
/// this class represent an trigger.
/// </summary>

[JsonPolymorphic(
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(TriggerOnConditionChange), nameof(TriggerOnConditionChange))]
[JsonDerivedType(typeof(TriggerTest), nameof(TriggerTest))]
[JsonDerivedType(typeof(TriggerEnd), nameof(TriggerEnd))]
[JsonDerivedType(typeof(Trigger), nameof(Trigger))]
public interface ITrigger : IDisposable
{
    /// <summary>
    /// This function is called when the trigger need to be actived.
    /// </summary>
    /// <param name="finisher">Save and call the finisher when your event get trigered the bool disposes this trigger</param>
    public void Activate(Action<bool> finisher);

    public List<(ConditionFlag, bool)>? GetConditions() { return null; }

    public string GetName()
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