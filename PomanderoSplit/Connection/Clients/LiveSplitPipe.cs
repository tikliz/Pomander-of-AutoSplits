using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;

using PomanderoSplit.Utils;

namespace PomanderoSplit.Connection;

public class LiveSplitPipe : IClient
{
    private ThreadSafeEnum<ClientStatus> Status { get; set; } = new(ClientStatus.Disconnected);
    private NamedPipeClientStream? Pipe { get; set; } = null;
    private readonly Mutex ClientLock = new();

    public ClientStatus GetStatus() => Status.Value;

    public void Connect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Status.Value != ClientStatus.Disconnected)
            {
                Dalamud.Log.Warning("LiveSplitPipe Connect, Is already connected");
                return;
            };

            if (!TryConnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Error($"LiveSplitPipe Connect: Connection failed");
                return;
            }

            Status.Value = ClientStatus.Connected;
            Dalamud.Log.Info($"LiveSplitPipe Connect: Done");
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
                Dalamud.Log.Warning("LiveSplitPipe Disconnect, Is not connected");
                return;
            };

            if (!TryDisconnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Warning($"LiveSplitPipe Disconnect: failed");
                return;
            }

            Status.Value = ClientStatus.Disconnected;
            Dalamud.Log.Info($"LiveSplitPipe Disconnect: Done");
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
                Dalamud.Log.Warning("LiveSplitPipe Reconnect, Is not connected");
                return;
            };

            if (!TryDisconnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Warning("LiveSplitPipe Reconnect: Failed Disconnect");
                return;
            }

            if (!TryConnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Warning("LiveSplitPipe Reconnect: Failed Connect");
                return;
            }

            Status.Value = ClientStatus.Connected;
            Dalamud.Log.Info("LiveSplitPipe Reconnect: Done");
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
            if (Pipe == null) throw new InvalidOperationException("Client is null");
            if (Status.Value != ClientStatus.Connected) throw new InvalidOperationException("Is not connected");

            using var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            Task.Run(() => Pipe.WriteAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message.ToString())), cancelSource.Token), cancelSource.Token).Wait(cancelSource.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"LiveSplitPipe Send, message \'{message}\':\n{ex}");
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

    private bool TryConnect()
    {
        try
        {
            if (Pipe != null) throw new InvalidOperationException("Client is not null");

            using var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            do
            {
                
                Pipe = new(".", "LiveSplit", PipeDirection.InOut, PipeOptions.Asynchronous);
                try
                {
                    Pipe.ConnectAsync(cancelSource.Token).Wait(cancelSource.Token);   
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Dalamud.Log.Info($"LiveSplitPipe TryConnect, ConnectAsync: \'{ex.Message}\'");
                    Pipe?.Dispose();
                    Pipe = null;
                    Task.Delay(TimeSpan.FromMilliseconds(50), cancelSource.Token).Wait(cancelSource.Token);
                    continue;
                }

                for (var delays = 0; delays != 4 && !Pipe.IsConnected; ++delays)
                {
                    Task.Delay(TimeSpan.FromMilliseconds(50), cancelSource.Token).Wait(cancelSource.Token);
                }
                if (Pipe.IsConnected) return true;

                Task.Delay(TimeSpan.FromMilliseconds(50), cancelSource.Token).Wait(cancelSource.Token);

                Pipe?.Dispose();
                Pipe = null;
            }
            while (!cancelSource.IsCancellationRequested);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"LiveSplitPipe TryConnect:\n{ex}");
        }
        Pipe?.Dispose();
        Pipe = null;
        return false;
    }

    private bool TryDisconnect()
    {
        try
        {
            if (Pipe == null) throw new InvalidOperationException("Client is null");
            if (!Pipe.IsConnected) throw new InvalidOperationException("Is not connected");

            using var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            do
            {
                Task.Run(() => Pipe.Close(), cancelSource.Token).Wait(cancelSource.Token);

                if (!Pipe.IsConnected) return true;
                Task.Delay(TimeSpan.FromMilliseconds(250), cancelSource.Token).Wait(cancelSource.Token);
            }
            while (!Pipe.IsConnected);

            Dalamud.Log.Warning($"LiveSplitPipe TryDisconnect: Forced done");
            return true; // Client.State == WebSocketState.Closed;
        }
        catch (OperationCanceledException) { return true; }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"LiveSplitPipe TryDisconnect, ClientState {((Pipe?.IsConnected ?? false) ? "true" : "false")}:\n{ex}");
            return false;
        }
        finally
        {
            Pipe?.Dispose();
            Pipe = null;
        }
    }

    private void ForceDisconnect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Pipe == null) return;
            if (!Pipe.IsConnected) return;
            _ = TryDisconnect();
        }
        finally
        {
            Pipe?.Dispose();
            Pipe = null;
            Status.Value = ClientStatus.Disconnected;
            ClientLock.ReleaseMutex();
        }
    }
}