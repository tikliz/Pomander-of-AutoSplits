using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.ImGuiNotification;

namespace PomanderoSplit.Utils;

public static class LogHelper
{
    public static void Info(string message)
    {
        Dalamud.Log.Info(message);
    }

    public static void Error(string message)
    {
        Dalamud.Log.Error(message);
    }

    public static void ReportSuccess(string message, int time = 20)
    {
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload(message), new UIForegroundPayload(0)));
        Dalamud.Notifications.AddNotification(new()
        {
            Title = message,
            InitialDuration = TimeSpan.FromSeconds(time),
            Type = NotificationType.Success
        });
    }

    public static void ReportFailure(string message, int time = 20)
    {
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload(message), new UIForegroundPayload(0)));
        Dalamud.Notifications.AddNotification(new()
        {
            Title = message,
            InitialDuration = TimeSpan.FromSeconds(time),
            Type = NotificationType.Error
        });
    }

    public static void ReportInfo(string message, int time = 5)
    {
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(12), new TextPayload(message), new UIForegroundPayload(0)));
        Dalamud.Notifications.AddNotification(new()
        {
            Title = message,
            InitialDuration = TimeSpan.FromSeconds(time),
            Type = NotificationType.Info
        });
    }
}