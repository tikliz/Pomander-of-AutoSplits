using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

using PomanderoSplit.Utils;

namespace PomanderoSplit.Connection;

public class LiveSplitSocket : IClient
{
    public Uri? Uri { get; set; } = null;

    private ThreadSafeEnum<ClientStatus> Status { get; set; } = new(ClientStatus.Disconnected);
    private Socket? Client { get; set; } = null;
    private readonly Mutex ClientLock = new();

    public ClientStatus GetStatus() => Status.Value;

    public void Connect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Status.Value != ClientStatus.Disconnected)
            {
                Dalamud.Log.Warning("LiveSplitSocket Connect, Is already connected");
                return;
            };

            if (!TryConnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Error($"LiveSplitSocket Connect: Connection failed");
                return;
            }

            Status.Value = ClientStatus.Connected;
            Dalamud.Log.Info($"LiveSplitSocket Connect: Done");
        }
        finally
        {
            ClientLock.ReleaseMutex();
        }
    }

    public void Disconnect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Status.Value != ClientStatus.Connected)
            {
                Dalamud.Log.Warning("LiveSplitSocket Disconnect, Is not connected");
                return;
            };

            if (!TryDisconnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Warning($"LiveSplitSocket Disconnect: failed");
                return;
            }

            Status.Value = ClientStatus.Disconnected;
            Dalamud.Log.Info($"LiveSplitSocket Disconnect: Done");
        }
        finally
        {
            ClientLock.ReleaseMutex();
        }
    }

    public void Reconnect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Status.Value != ClientStatus.Connected)
            {
                Dalamud.Log.Warning("LiveSplitSocket Reconnect, Is not connected");
                return;
            };

            if (!TryDisconnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Warning("LiveSplitSocket Reconnect: Failed Disconnect");
                return;
            }

            if (!TryConnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Warning("LiveSplitSocket Reconnect: Failed Connect");
                return;
            }

            Status.Value = ClientStatus.Connected;
            Dalamud.Log.Info("LiveSplitSocket Reconnect: Done");
        }
        finally
        {
            ClientLock.ReleaseMutex();
        }
    }

    public void Send(string message)
    {
        ClientLock.WaitOne();
        try
        {
            if (Client == null) throw new InvalidOperationException("Client is null");
            if (Status.Value != ClientStatus.Connected) throw new InvalidOperationException("Is not connected");

            using var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            Task.Run(() => Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message.ToString())), cancelSource.Token), cancelSource.Token).Wait(cancelSource.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"LiveSplitSocket Send, message \'{message}\':\n{ex}");
            Disconnect();
        }
        finally
        {
            ClientLock.ReleaseMutex();
        }
    }

    public void Dispose()
    {
        ClientLock.WaitOne();
        try
        {
            ForceDisconnect();
        }
        finally
        {
            ClientLock.Dispose();
        }
    }

    public bool ChangeUri(string addres)
    {
        ClientLock.WaitOne();
        try
        {
            if (string.IsNullOrEmpty(addres)) return false;
            if (Uri?.OriginalString == addres) return false;

            var newUri = new Uri(addres);            
            Uri = newUri;

            return true;
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"LiveSplitSocket ChangeUri, Uri {Uri?.OriginalString ?? "null"}, addres {addres}:\n{ex}");
            return false;
        }
        finally
        {
            ClientLock.ReleaseMutex();
        }
    }


    private bool TryConnect()
    {
        try
        {
            if (Uri == null) throw new InvalidOperationException("Uri is null");
            if (Client != null) throw new InvalidOperationException("Client is not null");

            using var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            do
            {
                Client = new(SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    Task.Run(() => Client.ConnectAsync(Uri.Host, Uri.Port, cancelSource.Token), cancelSource.Token).Wait(cancelSource.Token);   
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Dalamud.Log.Info($"LiveSplitSocket TryConnect, ConnectAsync: \'{ex.Message}\'");
                    Client?.Dispose();
                    Client = null;
                    Task.Delay(TimeSpan.FromMilliseconds(50), cancelSource.Token).Wait(cancelSource.Token);
                    continue;
                }

                for (var delays = 0; delays != 4 && !Client.Connected; ++delays)
                {
                    Task.Delay(TimeSpan.FromMilliseconds(50), cancelSource.Token).Wait(cancelSource.Token);
                }
                if (Client.Connected) return true;

                Task.Delay(TimeSpan.FromMilliseconds(50), cancelSource.Token).Wait(cancelSource.Token);

                Client?.Dispose();
                Client = null;
            }
            while (!cancelSource.IsCancellationRequested);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"LiveSplitSocket TryConnect:\n{ex}");
        }
        Client?.Dispose();
        Client = null;
        return false;
    }

    private bool TryDisconnect()
    {
        try
        {
            if (Client == null) throw new InvalidOperationException("Client is null");
            if (!Client.Connected) throw new InvalidOperationException("Is not connected");

            using var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            do
            {
                Task.Run(() => Client.DisconnectAsync(false, cancelSource.Token), cancelSource.Token).Wait(cancelSource.Token);

                if (!Client.Connected) return true;
                Task.Delay(TimeSpan.FromMilliseconds(250), cancelSource.Token).Wait(cancelSource.Token);
            }
            while (!Client.Connected);

            Dalamud.Log.Warning($"LiveSplitSocket TryDisconnect: Forced done");
            return true; // Client.State == WebSocketState.Closed;
        }
        catch (OperationCanceledException) { return true; }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"LiveSplitSocket TryDisconnect, ClientState {((Client?.Connected ?? false) ? "true" : "false")}:\n{ex}");
            return false;
        }
        finally
        {
            Client?.Dispose();
            Client = null;
        }
    }

    private void ForceDisconnect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Client == null) return;
            if (!Client.Connected) return;
            _ = TryDisconnect();
        }
        finally
        {
            Client?.Dispose();
            Client = null;
            Status.Value = ClientStatus.Disconnected;
            ClientLock.ReleaseMutex();
        }
    }
}