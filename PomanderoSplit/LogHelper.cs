namespace PomanderoSplit;

public class LogHelper
{
    public void Info(string message)
    {
        Plugin.Log.Info(message);
    }

    public void Error(string message)
    {
        Plugin.Log.Error(message);
    }
}