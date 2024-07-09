using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;

namespace PomanderoSplit.RunHandler;

public class Trigger : ITrigger
{
    public Action<bool> Finisher { get; set; } = (_) => { };
    private bool Activated { get; set; } = false;

    public void Activate(Action<bool> finisher)
    {
        Finisher = finisher;
        Activated = true;
        Dalamud.Conditions.ConditionChange += OnConditionChange;
    }

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        if (flag == ConditionFlag.Mounted && value) Finisher.Invoke(true);
    }

    public void Dispose()
    {
        if (!Activated) return;
        Dalamud.Conditions.ConditionChange -= OnConditionChange;
    }
}

public class TriggerEnd : ITrigger
{
    public Action<bool> Finisher { get; set; } = (_) => { };
    private bool Activated { get; set; } = false;
    
    private List<(ConditionFlag, bool)> Flags { get; set; }

    public TriggerEnd(List<(ConditionFlag, bool)> flags)
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
}


public class TriggerTest : ITrigger
{
    public Action<bool> Finisher { get; set; } = (_) => { };
    private bool Activated { get; set; } = false;
    
    private List<(ConditionFlag, bool)> Flags { get; set; }
    private int count = 0;
    private int countMax;
    private bool endSuccess;

    public TriggerTest(List<(ConditionFlag, bool)> flags, int countMax = 3, bool endSuccess = true)
    {
        this.endSuccess = endSuccess;
        Flags = flags;
        this.countMax = countMax;
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
                count++;
                if (count >= countMax)
                {
                    count = 0;
                    Finisher.Invoke(endSuccess);
                }
            }
        }
    }

    public void Dispose()
    {
        if (!Activated) return;
        Dalamud.Conditions.ConditionChange -= OnConditionChange;
    }
}