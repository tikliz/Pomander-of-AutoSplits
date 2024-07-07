using System;
using System.Net.Sockets;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.ImGuiNotification;

namespace PomanderoSplit;

public class LiveSplitClient : IDisposable
{
    private Socket? SocketClient { get; set; }
    public int Port { get; private set; }

    private const int ConnectionAttempts = 3;

    public LiveSplitClient(Plugin plugin)
    {
        Port = plugin.Configuration.LivesplitPort;
    }

    public void ChangePort(int Value)
    {
        Port = Value;
        Reconnect();
    }

    public bool Status() => SocketClient?.Connected ?? false;

    public void Connect()
    {
        if (Status())
        {
            Dalamud.Log.Warning("Already connected.");
            return;
        }

        for (var attempt = 0; attempt != ConnectionAttempts; ++attempt)
        {
            try
            {
                SocketClient?.Dispose(); // redundancy
                SocketClient = new(SocketType.Stream, ProtocolType.Tcp);
                SocketClient.Connect("localhost", Port);
            }
            catch (Exception ex)
            {
                Dalamud.Log.Debug($"Connect error: {ex}");
                SocketClient?.Dispose();
                SocketClient = null;
            }

            // TODO: Add delay betwen attempts

            if (Status())
            {
                Dalamud.Log.Info("Connection was successful.");
                ReportSuccess("Connection was successful.");
                return;
            }

            SocketClient?.Dispose();
            SocketClient = null;

            Dalamud.Log.Info($"Connection to port \'{Port}\', attempt {attempt}/{ConnectionAttempts}");
        }

        Dalamud.Log.Warning($"Failed to connect after {ConnectionAttempts} attempts");
        ReportFailure($"Failed to connect");
    }

    public void Disconnect()
    {
        if (!Status())
        {
            Dalamud.Log.Debug("LiveSplit connection is already off.");
            SocketClient?.Dispose();
            SocketClient = null;
            return;
        }

        try
        {
            SocketClient?.Shutdown(SocketShutdown.Both);
            SocketClient?.Close();
            SocketClient?.Disconnect(false);

            Dalamud.Log.Info("Closed socket client.");
            ReportSuccess("Closed socket client.");
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Disconnection error: {ex}");
        }
        finally
        {
            SocketClient?.Dispose();
            SocketClient = null;
        }
    }

    public void Reconnect()
    {
        Disconnect();
        Connect();
    }

    private void SendMessage(string message)
    {
        if (!Status())
        {
            Dalamud.Log.Debug("SendMessage was requested while not connected.");
            return;
        }

        var formated = message;
        if (!formated.EndsWith('\n')) formated += Environment.NewLine;

        var encoded = Encoding.ASCII.GetBytes(formated);

        try
        {
            Dalamud.Log.Debug($"Sending message: {message} | {encoded}");
            SocketClient?.Send(encoded);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"SendMessage error: {ex}");
            Disconnect();
        }
    }

    public void Dispose()
    {
        SocketClient?.Dispose();
    }
    
    public void Reset() => SendMessage("reset");

    public void StartOrSplit() => SendMessage("startorsplit");

    public void Pause() => SendMessage("pause");

    private static void ReportSuccess(string message)
    {
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload(message), new UIForegroundPayload(0)));
        Dalamud.Notifications.AddNotification(new()
        {
            Title = message,
            InitialDuration = TimeSpan.FromSeconds(20),
            Type = NotificationType.Success
        });
    }

    private static void ReportFailure(string message)
    {
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload(message), new UIForegroundPayload(0)));
        Dalamud.Notifications.AddNotification(new()
        {
            Title = message,
            InitialDuration = TimeSpan.FromSeconds(20),
            Type = NotificationType.Error
        });
    }

}