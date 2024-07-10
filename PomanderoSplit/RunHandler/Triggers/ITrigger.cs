using System;

namespace PomanderoSplit.RunHandler.triggers;

/// <summary>
/// this class represent an trigger.
/// </summary>
public interface ITrigger : IDisposable
{
    /// <summary>
    /// This function is called when the trigger need to be actived.
    /// </summary>
    /// <param name="finisher">Save and call the finisher when your event get trigered the bool disposes this trigger</param>
    public void Activate(Action<bool> finisher);
}

/*

public class TriggerTemplate : ITrigger
{
    public Action<bool> Finisher { get; set; } = (_) => { };
    private bool Activated { get; set; } = false;

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