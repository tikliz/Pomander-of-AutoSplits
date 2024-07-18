using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using PomanderoSplit.RunHandler.triggers;

namespace PomanderoSplit.RunHandler;

public enum RunState
{
    InActive,
    Active,
    Ready,
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

    // before changing the BeginRunTriggers dont forget to: if (run.BeginRunTriggers != null) foreach (var trigger in run.BeginRunTriggers) trigger.Dispose();
    public ITrigger[]? BeginRunTriggers { get; set; }

    public RunState Status { get; private set; } = RunState.InActive;
    public List<TimeSpan> Splits { get; private set; } = [];

    public event EventHandler OnStatusChange = delegate { };
    public event EventHandler OnSplit = delegate { };

    private readonly object runLock = new();
    private Stopwatch RunStopwatch { get; set; } = new();

    public GenericRun(string name, Objective[] objectives, ITrigger[]? beginRun = null, bool activate = true)
    {
        Name = name;
        Objectives = objectives;
        BeginRunTriggers = beginRun;
        if (activate) Activate();
    }

    public Objective CurrentObjective() => Objectives[Splits.Count];
    public TimeSpan Elapsed() => RunStopwatch.Elapsed;

    public void Activate()
    {
        lock (runLock)
        {
            if (BeginRunTriggers == null) return;
            if (Status != RunState.InActive) throw new InvalidOperationException($"GenericRun Activate, Name {Name}, Status {Status}: can not Activate the run in this state");

            foreach (var trigger in BeginRunTriggers)
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
                            Dalamud.Log.Error($"Objective Trigger {Name} Finisher: {ex}");
                            Dispose();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dalamud.Log.Error($"Objective Trigger {Name} Activation, index {Array.IndexOf(BeginRunTriggers, trigger)}: {ex}");
                }
            }
            Status = RunState.Ready;
        }
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

            if (Status == RunState.Paused)
            {
                Status = RunState.Active;
                OnStatusChange.Invoke(this, EventArgs.Empty);
                CurrentObjective().Dispose();
                Splits.Add(RunStopwatch.Elapsed);
                CurrentObjective().Init(this);
                OnSplit.Invoke(this, EventArgs.Empty);
                Status = RunState.Paused;
                OnStatusChange.Invoke(this, EventArgs.Empty);
                Dalamud.Log.Debug($"GenericRun Split, before pause Name {Name}: Done");
            }
            else
            {
                CurrentObjective().Dispose();
                Splits.Add(RunStopwatch.Elapsed);
                CurrentObjective().Init(this);
                OnSplit.Invoke(this, EventArgs.Empty);
                Dalamud.Log.Debug($"GenericRun Split, after pause Name {Name}: Done");
            }
        }
    }

    public void Pause()
    {
        lock (runLock)
        {
            if (Status == RunState.Paused) return;
            if (Status != RunState.Active && Status != RunState.Paused) throw new InvalidOperationException($"GenericRun Pause, Name {Name}, Status {Status}: can not pause the run in this state");

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
            if (Status == RunState.Active) return;
            if (Status != RunState.Paused && Status != RunState.Active) throw new InvalidOperationException($"GenericRun Resume, Name {Name}, Status {Status}: can not resume the run in this state");

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

            Dalamud.Log.Debug($"GenericRun End, Name {Name}, CompletedSuccessfullya {(CompletedSuccessfully ? "true" : "false")}: Done");
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