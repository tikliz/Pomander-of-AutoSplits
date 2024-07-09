using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PomanderoSplit.RunHandler;

public enum RunState
{
    InActive,
    Active,
    Paused,
    Completed,
    Failed,
}

[Serializable]
public class GenericRun : IDisposable
{
    private Stopwatch RunStopwatch { get; set; } = new();

    public string Name = string.Empty;

    public RunState Status = RunState.InActive;

    public Objective[] Objectives { get; private init; }
    public List<TimeSpan> Splits { get; private set; } = [];

    public ITrigger? BeginRunTrigger { get; init; }
    
    public Objective CurrentObjective() => Objectives[Splits.Count];
    public int ObjectivesLen() => Objectives.Length;
    public TimeSpan Duration() => RunStopwatch.Elapsed;
    
    public GenericRun(string name, Objective[] objectives, ITrigger? beginRun = null)
    {
        Name = name;
        Objectives = objectives;
        BeginRunTrigger = beginRun;
        BeginRunTrigger?.Activate((_) => { Begin(); BeginRunTrigger?.Dispose(); });
    }

    public event EventHandler OnSplit = delegate { };

    public void Begin()
    {
        if (Status != RunState.InActive) throw new InvalidOperationException($"GenericRun Begin, Name {Name}, Status {Status}: can not begin the run in this state");

        RunStopwatch.Start();
        CurrentObjective().Init(this);
        Status = RunState.Active;

        Dalamud.Log.Debug($"GenericRun Begin, {Name}: Done");
    }

    public void Split()
    {
        if (Status != RunState.Active && Status != RunState.Paused) throw new InvalidOperationException($"GenericRun Split, Name {Name}, Status {Status}: can not split the run in this state");

        if (Splits.Count > ObjectivesLen()) throw new InvalidOperationException($"GenericRun Split, Name {Name}, Splits.Count {Splits.Count}: can not split the run anymore");

        Splits.Add(RunStopwatch.Elapsed);
        CurrentObjective().Init(this);

        OnSplit.Invoke(null, EventArgs.Empty);

        Dalamud.Log.Debug($"GenericRun Split, {Name}: Done");
    }

    public void Pause()
    {
        if (Status != RunState.Active) throw new InvalidOperationException($"GenericRun Pause, Name {Name}, Status {Status}: can not pause the run in this state");

        RunStopwatch.Stop();
        Status = RunState.Paused;

        Dalamud.Log.Debug($"GenericRun Pause, {Name}: Done");
    }

    public void Resume()
    {
        if (Status != RunState.Paused) throw new InvalidOperationException($"GenericRun Resume, Name {Name}, Status {Status}: can not resume the run in this state");

        RunStopwatch.Start();
        Status = RunState.Active;

        Dalamud.Log.Debug($"GenericRun Continue, {Name}: Done");
    }

    public void End(bool CompletedSuccessfully = false)
    {
        if (Status == RunState.Completed || Status == RunState.Failed) throw new InvalidOperationException($"GenericRun Finish, Status {Status}: can not finish the run in this state");

        RunStopwatch.Stop();
        Status = CompletedSuccessfully ? RunState.Completed : RunState.Failed;
        Dispose();

        Dalamud.Log.Debug($"GenericRun End, {Name}, CompletedSuccessfully {(CompletedSuccessfully ? "true" : "false")}: Done");
    }

    public override string ToString() => "";
    
    public void Dispose()
    {
        foreach(var objective in Objectives) objective.Dispose();
    }
}