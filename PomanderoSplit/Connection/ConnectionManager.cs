using System;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.ImGuiNotification;
using PomanderoSplit.Utils;

namespace PomanderoSplit.Connection;

/// <summary>
/// Enumeration representing the types of clients that can be used to connect.
/// </summary>
public enum ClientType
{
    Pipe,
    Socket
}

/// <summary>
/// Manages the connection.
/// </summary>
public class ConnectionManager : IDisposable
{
    private Plugin Plugin { get; init; }

    /// <summary>
    /// The client used to communicate with the server.
    /// </summary>
    public IClient? Client { get; private set; }

    public ConnectionManager(Plugin plugin)
    {
        Plugin = plugin;

        Init();
        if (Plugin.Configuration.AutoConnect) AutoConnect();
    }

    /// <summary>
    /// Gets the current status of the connection.
    /// </summary>
    /// <returns>The current status of the connection.</returns>
    public ClientStatus Status() => Client?.GetStatus() ?? ClientStatus.NotInitialized;

    /// <summary>
    /// Connects.
    /// </summary>
    public void Connect() => Task.Run(() =>
    {
        if (Client == null || Client.GetStatus() == ClientStatus.Connected) return;

        Client.Connect();

        if (Client.GetStatus() != ClientStatus.Connected)
        {
            LogHelper.ReportFailure("PomanderoSplit could not connect.");
            return;
        }

        LogHelper.ReportSuccess("PomanderoSplit is connected.");
    });

    /// <summary>
    /// Disconnects.
    /// </summary>
    public void Disconnect() => Task.Run(() =>
    {
        if (Client == null || Client.GetStatus() == ClientStatus.Disconnected) return;

        Client.Disconnect();

        if (Client.GetStatus() != ClientStatus.Disconnected)
        {
            LogHelper.ReportFailure("PomanderoSplit could not disconnect.");
            return;
        }
        LogHelper.ReportSuccess("PomanderoSplit is disconnected.");
    });

    /// <summary>
    /// Reconnects.
    /// </summary>
    public void Reconnect() => Task.Run(() =>
    {
        if (Client == null || Client.GetStatus() != ClientStatus.Connected) return;
        Client.Reconnect();

        if (Client.GetStatus() != ClientStatus.Connected)
        {
            LogHelper.ReportFailure("PomanderoSplit could not reconnect.");
            return;
        }
        LogHelper.ReportSuccess("PomanderoSplit is reconnected.");
    });

    /// <summary>
    /// Disposes of the connection manager.
    /// </summary>
    public void Dispose()
    {
        Client?.Dispose();
    }

    /// <summary>
    /// Initializes the connection manager.
    /// </summary>
    public void Init()
    {
        try
        {
            if (Client != null)
            {
                if (Client.GetStatus() == ClientStatus.Connected) Client.Disconnect();
                Client.Dispose();
                Client = null;
            };

            

            Client = Plugin.Configuration.ClientType switch
            {
                ClientType.Pipe => new LiveSplitPipe(),
                ClientType.Socket => new LiveSplitSocket()
                {
                    Uri = new(Plugin.Configuration.Address)
                },
                _ => throw new Exception("Client is null")
            };
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ConnectionManager Init:\n{ex}");
            Client?.Dispose();
            Client = null;
        }
    }

    private void Send(string message) => Task.Run(() => Client?.Send(message));

    private void AutoConnect()
    {
        void OnLogin(object? _)
        {
            if (!Dalamud.ClientState.IsLoggedIn) return;
            Dalamud.Framework.Update -= OnLogin;
            Connect();
        }

        if (Dalamud.ClientState.IsLoggedIn) Connect();
        else Dalamud.Framework.Update += OnLogin;
    }

    public void StartOrSplit() => Send("startorsplit\n");

    public void Start() => Send("play\n");

    public void Pause() => Send("pause\n");
    
    public void Reset() => Send("reset\n");

    public void Split() => Send("split\n");

    public void Resume() => Send("resume\n");
}
