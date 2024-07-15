using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using PomanderoSplit.RunHandler.triggers;
using System.Text.Json.Serialization;

namespace PomanderoSplit.RunHandler;

public enum RunState
{
    InActive,
    Active,
    Paused,
    Completed,
    Failed,
    Reset,
}

[Serializable]
public class GenericRun : IDisposable
{
    public string Name { get; set; }
    public Objective[] Objectives { get; set; }
    public ITrigger[]? BeginRunTriggers { get; set; }

    [JsonInclude]
    private bool IsPreset { get; set; }

    public RunState Status { get; private set; } = RunState.InActive;
    public List<TimeSpan> Splits { get; private set; } = [];

    public event EventHandler OnStatusChange = delegate { };
    public event EventHandler OnSplit = delegate { };

    private readonly object runLock = new();
    private Stopwatch RunStopwatch { get; set; } = new();

    public GenericRun(string name, Objective[] objectives, ITrigger[]? beginRun = null, bool isPreset = false)
    {
        Name = name;
        Objectives = objectives;
        BeginRunTriggers = beginRun;
        IsPreset = isPreset;

        if (BeginRunTriggers != null && !IsPreset) foreach (var trigger in BeginRunTriggers)
        {
            try
            {
                trigger.Activate((value) =>
                {
                    try
                    {
                        Begin(); 
                        if (BeginRunTriggers != null) foreach (var trigger in BeginRunTriggers) trigger.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Dalamud.Log.Error($"Objective Trigger {name} Finisher: {ex}");
                        Dispose();
                    }
                });
            }
            catch (Exception ex)
            {
                Dalamud.Log.Error($"Objective Trigger {name} Activation, index {Array.IndexOf(BeginRunTriggers, trigger)}: {ex}");
            }
        }
    }

    public Objective CurrentObjective() => Objectives[Splits.Count];
    public TimeSpan Elapsed() => RunStopwatch.Elapsed;

    public void SetBeginTriggers(ITrigger[] triggers)
    {
        this.BeginRunTriggers = triggers;
    }

    public void Begin()
    {
        lock (runLock)
        {
            if (Status != RunState.InActive) throw new InvalidOperationException($"GenericRun Begin, Name {Name}, Status {Status}: can not begin the run in this state");

            RunStopwatch.Start();
            CurrentObjective().Init(this);

            OnStatusChange.Invoke(this, EventArgs.Empty);
            Status = RunState.Active;

            Dalamud.Log.Debug($"GenericRun Begin, Name {Name}: Done");
        }
    }

    public void Split()
    {
        lock (runLock)
        {
            if (Status != RunState.Active && Status != RunState.Paused) throw new InvalidOperationException($"GenericRun Split, Name {Name}, Status {Status}: can not split the run in this state");

            if (Splits.Count > Objectives.Length) throw new InvalidOperationException($"GenericRun Split, Name {Name}, Splits.Count {Splits.Count}: can not split the run anymore");

            Splits.Add(RunStopwatch.Elapsed);
            CurrentObjective().Init(this);

            OnSplit.Invoke(this, EventArgs.Empty);

            Dalamud.Log.Debug($"GenericRun Split, Name {Name}: Done");
        }
    }

    public void Pause()
    {
        lock (runLock)
        {
            if (Status != RunState.Active) throw new InvalidOperationException($"GenericRun Pause, Name {Name}, Status {Status}: can not pause the run in this state");

            RunStopwatch.Stop();
            Status = RunState.Paused;

            OnStatusChange.Invoke(this, EventArgs.Empty);

            Dalamud.Log.Debug($"GenericRun Pause, Name {Name}: Done");
        }
    }

    public void Resume()
    {
        lock (runLock)
        {
            if (Status != RunState.Paused) throw new InvalidOperationException($"GenericRun Resume, Name {Name}, Status {Status}: can not resume the run in this state");

            RunStopwatch.Start();
            Status = RunState.Active;

            OnStatusChange.Invoke(this, EventArgs.Empty);

            Dalamud.Log.Debug($"GenericRun Resume, Name {Name}: Done");
        }
    }

    public void End(bool CompletedSuccessfully = false)
    {
        lock (runLock)
        {
            if (Status == RunState.Completed || Status == RunState.Failed) throw new InvalidOperationException($"GenericRun Finish, Status {Status}: can not finish the run in this state");

            RunStopwatch.Stop();
            Status = CompletedSuccessfully ? RunState.Completed : RunState.Failed;
            Dispose();
            
            OnStatusChange.Invoke(this, EventArgs.Empty);

            Dalamud.Log.Debug($"GenericRun End, Name {Name}, CompletedSuccessfully {(CompletedSuccessfully ? "true" : "false")}: Done");
        }
    }

    public void Reset()
    {
        lock (runLock)
        {
            // if (Status == RunState.Completed || Status == RunState.Failed) throw new InvalidOperationException($"GenericRun Finish, Status {Status}: can not finish the run in this state");

            RunStopwatch.Stop();
            Status = RunState.Reset;
            Dispose();
            
            OnStatusChange.Invoke(this, EventArgs.Empty);

            Dalamud.Log.Debug($"GenericRun Reset, Name {Name}, Status {Status}: Done");
        }
    }

    public void SetObjectives(Objective[] objectives)
    {
        Objectives = objectives;
    }

    public override string ToString()
    {
        var objectivesString = Objectives != null ? string.Join(", ", Objectives.ToString()) : "None";
        var splitsString = Splits.Count > 0 ? string.Join(", ", Splits) : "None";

        var sb = new StringBuilder();
        sb.AppendLine($"Name: {Name}");
        sb.AppendLine($"Objectives: {objectivesString}");
        sb.AppendLine($"Status: {Status}");
        sb.AppendLine($"Splits: {splitsString}");
        return sb.ToString();
    }

    public void Dispose()
    {
        lock (runLock)
        {
            if (BeginRunTriggers != null) foreach (var trigger in BeginRunTriggers) trigger.Dispose();
            foreach (var objective in Objectives) objective.Dispose();

            if (Status != RunState.Completed && Status != RunState.Failed && Status != RunState.Reset) End();
        }
    }
}