namespace PomanderoSplit;

public class LogHelper
{
    public void Info(string message)
    {
        Dalamud.Log.Info(message);
    }

    public void Error(string message)
    {
        Dalamud.Log.Error(message);
    }
}