using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace PomanderoSplit;

public class LiveSplitClient
{
    private Configuration configuration;
    private IChatGui chatGui;
    private INotificationManager notificationManager;
    public Socket livesplitSocket = new(SocketType.Stream, ProtocolType.Tcp);
    public int port { get; set; }

    const int CONNECTIONTIMEOUT = 3;
    private int connectionTimeoutCount = 0;

    public LiveSplitClient(Plugin plugin, IChatGui chatGui, INotificationManager notificationManager)
    {
        this.chatGui = chatGui;
        this.notificationManager = notificationManager;
        configuration = plugin.Configuration;
        port = configuration.LivesplitPort;
    }

    internal void UpdatePort()
    {
        port = configuration.LivesplitPort;
    }

    internal void Connect()
    {
        try
        {
            // Connect to the server.
            if (!livesplitSocket.Connected)
            {
                livesplitSocket.Connect("localhost", port);
                Plugin.Log.Info("Connection was successful.");
                ReportSuccess("Connection was successful.");
            }
            else
            {
                Plugin.Log.Info("Already connected.");
            }
        }
        catch (ObjectDisposedException)
        {
            Plugin.Log.Warning($"Connection was closed, recreating socket and retrying. {connectionTimeoutCount}/{CONNECTIONTIMEOUT}");
            livesplitSocket = new(SocketType.Stream, ProtocolType.Tcp);
            if (connectionTimeoutCount < CONNECTIONTIMEOUT)
            {
                connectionTimeoutCount += 1;
                Connect();
            }
            else
            {
                Plugin.Log.Error($"Failed to connect after {connectionTimeoutCount} tries");
                ReportFailure($"Failed to connect after {connectionTimeoutCount} tries");
            }
        }
        catch (Exception ex)
        {
            
            Plugin.Log.Error($"Failed to connect: {ex}\n\n -- Please make sure the LiveSplit TCP server is up and running on PORT [{port}] --.");
            ReportFailure("Failed to connect: Please make sure the LiveSplit TCP server is up and running on the right port.");
        }
    }

    internal void Disconnect()
    {
        if (livesplitSocket != null && livesplitSocket.Connected)
        {
            livesplitSocket.Shutdown(SocketShutdown.Both);
            livesplitSocket.Close();
            Plugin.Log.Info("Closed socket.");
            ReportSuccess("Closed socket.");
        }
        else
        {
            Plugin.Log.Debug("LiveSplit connection is already off.");
        }
    }

    internal void Reconnect()
    {
        Disconnect();
        Connect();
    }

    public void SendMessage(string message)
    {
        if (!livesplitSocket.Connected)
        {
            Plugin.Log.Debug("SendMessage was requested while not connected.");
            return;
        }
        // Convert the string message to a byte array.
        string formatted_string = message;
        if (!formatted_string.EndsWith('\n'))
        {
            formatted_string = formatted_string + Environment.NewLine;
        }
        byte[] byte_message = Encoding.ASCII.GetBytes(formatted_string);
        Plugin.Log.Info($"Sending message: {message} | {byte_message}");

        // Send the message to the server.
        livesplitSocket.Send(byte_message);
    }

    private void ReportSuccess(string message)
    {
        chatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload(message), new UIForegroundPayload(0)));
        notificationManager.AddNotification(new()
        {
            Title = message,
            InitialDuration = TimeSpan.FromSeconds(20),
            Type = NotificationType.Success
        });
    }

    private void ReportFailure(string message)
    {
        chatGui.Print(new SeString(new UIForegroundPayload(73), new TextPayload(message), new UIForegroundPayload(0)));
        notificationManager.AddNotification(new()
        {
            Title = message,
            InitialDuration = TimeSpan.FromSeconds(20),
            Type = NotificationType.Error
        });
    }
}