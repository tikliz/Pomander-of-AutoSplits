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
    [JsonInclude]
    private bool successfulEnd = false;

    public TriggerOnConditionChange()
    {
        Flags = [];
    }

    public TriggerOnConditionChange(List<(ConditionFlag, bool)> flags, bool successfulEnd = false)
    {
        Flags = flags;
        this.successfulEnd = successfulEnd;
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
            if ((flag == cflag) && (value == !cval))
            {
                Finisher.Invoke(successfulEnd);
                break;
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
        if (Flags.Count == 0) return null;
        return Flags;
    }

    public void SetConditions(List<(ConditionFlag, bool)> flags)
    {
        Flags = flags;
    }

    public string GetTypeName()
    {
        string name = this.GetType().Name;
        if (name.StartsWith("Trigger"))
        {
            name = name.Substring(7);
        }
        return name;
    }
}


public class TriggerOnDutyComplete : ITrigger
{
    [JsonIgnore]
    public Action<bool> Finisher { get; set; } = (_) => { };
    private bool Activated { get; set; } = false;
    
    [JsonInclude]
    private List<(ConditionFlag, bool)> Flags { get; set; }

    public TriggerOnDutyComplete()
    {
        Flags = [];
    }

    public TriggerOnDutyComplete(List<(ConditionFlag, bool)> flags)
    {
        Flags = flags;
    }

    public void Activate(Action<bool> finisher)
    {
        Finisher = finisher;
        Activated = true;
        Dalamud.Duty.DutyCompleted += OnDutyComplete;
    }

    private void OnDutyComplete(object? sender, ushort e)
    {
        foreach ((var cflag, var cvalue) in Flags)
        {
            if (Dalamud.Conditions[cflag] == cvalue)
            {
                Finisher.Invoke(true);
                break;
            }
        }
    }

    public void Dispose()
    {
        if (!Activated) return;
        Dalamud.Duty.DutyCompleted -= OnDutyComplete;
    }

    public List<(ConditionFlag, bool)>? GetConditions()
    {
        if (Flags.Count == 0) return null;
        return Flags;
    }

    public void SetConditions(List<(ConditionFlag, bool)> flags)
    {
        Flags = flags;
    }

    public string GetTypeName()
    {
        string name = this.GetType().Name;
        if (name.StartsWith("Trigger"))
        {
            name = name.Substring(7);
        }
        return name;
    }
}

public class TriggerOnDutyWiped : ITrigger
{
    [JsonIgnore]
    public Action<bool> Finisher { get; set; } = (_) => { };
    private bool Activated { get; set; } = false;
    
    [JsonInclude]
    private List<(ConditionFlag, bool)> Flags { get; set; }

    public TriggerOnDutyWiped()
    {
        Flags = [];
    }

    public TriggerOnDutyWiped(List<(ConditionFlag, bool)> flags)
    {
        Flags = flags;
    }

    public void Activate(Action<bool> finisher)
    {
        Finisher = finisher;
        Activated = true;
        Dalamud.Duty.DutyWiped += OnDutyWiped;
    }

    private void OnDutyWiped(object? sender, ushort e)
    {
        foreach ((var cflag, var cvalue) in Flags)
        {
            if (Dalamud.Conditions[cflag] == cvalue)
            {
                Finisher.Invoke(false);
                break;
            }
        }
    }

    public void Dispose()
    {
        if (!Activated) return;
        Dalamud.Duty.DutyWiped -= OnDutyWiped;
    }

    public List<(ConditionFlag, bool)>? GetConditions()
    {
        if (Flags.Count == 0) return null;
        return Flags;
    }

    public void SetConditions(List<(ConditionFlag, bool)> flags)
    {
        Flags = flags;
    }

    public string GetTypeName()
    {
        string name = this.GetType().Name;
        if (name.StartsWith("Trigger"))
        {
            name = name.Substring(7);
        }
        return name;
    }
}


public class TriggerOnDutyStarted : ITrigger
{
    [JsonIgnore]
    public Action<bool> Finisher { get; set; } = (_) => { };
    private bool Activated { get; set; } = false;
    
    [JsonInclude]
    private List<(ConditionFlag, bool)> Flags { get; set; }

    public TriggerOnDutyStarted()
    {
        Flags = [];
    }

    public TriggerOnDutyStarted(List<(ConditionFlag, bool)> flags)
    {
        Flags = flags;
    }

    public void Activate(Action<bool> finisher)
    {
        Finisher = finisher;
        Activated = true;
        Dalamud.Duty.DutyStarted += OnDutyStarted;
    }

    private void OnDutyStarted(object? sender, ushort e)
    {
        foreach ((var cflag, var cvalue) in Flags)
        {
            if (Dalamud.Conditions[cflag] == cvalue)
            {
                Finisher.Invoke(true);
                break;
            }
        }
    }

    public void Dispose()
    {
        if (!Activated) return;
        Dalamud.Duty.DutyStarted -= OnDutyStarted;
    }

    public List<(ConditionFlag, bool)>? GetConditions()
    {
        if (Flags.Count == 0) return null;
        return Flags;
    }

    public void SetConditions(List<(ConditionFlag, bool)> flags)
    {
        Flags = flags;
    }

    public string GetTypeName()
    {
        string name = this.GetType().Name;
        if (name.StartsWith("Trigger"))
        {
            name = name.Substring(7);
        }
        return name;
    }
}