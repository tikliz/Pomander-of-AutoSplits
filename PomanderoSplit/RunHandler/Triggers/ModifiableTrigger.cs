using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Dalamud.Game.ClientState.Conditions;

namespace PomanderoSplit.RunHandler.triggers;

public class TriggerOnConditionChange : ITrigger
{
    [JsonIgnore]
    public Action<bool> Finisher { get; set; } = (_) => { };
    private bool Activated { get; set; } = false;
    
    [JsonInclude]
    private List<(ConditionFlag, bool)> Flags { get; set; }

    public TriggerOnConditionChange(List<(ConditionFlag, bool)> flags)
    {
        Flags = flags;
    }

    public void Activate(Action<bool> finisher)
    {
        Finisher = finisher;
        Activated = true;
        Dalamud.Conditions.ConditionChange += OnConditionChange;
    }
    
    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        foreach (var (cflag, cval) in Flags)
        {
            if ((flag == cflag) && (value == cval))
            {
                Finisher.Invoke(false);
            }
        }
    }

    public void Dispose()
    {
        if (!Activated) return;
        Dalamud.Conditions.ConditionChange -= OnConditionChange;
    }

    public List<(ConditionFlag, bool)>? GetConditions()
    {
        return Flags;
    }

    public string GetName()
    {
        string name = this.GetType().Name;
        if (name.StartsWith("Trigger"))
        {
            name = name.Substring(7);
        }
        return name;
    }
}