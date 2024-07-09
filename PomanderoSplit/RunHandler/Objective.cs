using System;

namespace PomanderoSplit.RunHandler;

public class Objective : IDisposable
{
    public string Name { get; set; } = string.Empty;

    public ITrigger? Begin { get; set; } = null;
    public ITrigger? Split { get; set; } = null;
    public ITrigger? Pause { get; set; } = null;
    public ITrigger? Resume { get; set; } = null;
    public ITrigger? End { get; set; } = null;

    public void Dispose()
    {
        Begin?.Dispose();
        Split?.Dispose();
        Pause?.Dispose();
        Resume?.Dispose();
        End?.Dispose();
    }

    public void Init(GenericRun run)
    {
        Begin?.Activate((dispose) =>
        {
            run.Begin();
            if (dispose) Dispose();
        });
        Split?.Activate((dispose) =>
        {
            run.Split();
            if (dispose) Dispose();
        });
        Pause?.Activate((dispose) =>
        {
            run.Pause();
            if (dispose) Dispose();
        });
        Resume?.Activate((dispose) =>
        {
            run.Resume();
            if (dispose) Dispose();
        });
        End?.Activate((CompletedSuccessfully) =>
        {
            run.End(CompletedSuccessfully);
            Dispose();
        });

        Dalamud.Log.Debug("Objective Init: Done");
    }
}