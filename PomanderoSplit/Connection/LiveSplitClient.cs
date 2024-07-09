// using System;
// using System.IO;
// using System.IO.Pipes;
// using System.Net.Sockets;
// using System.Text;
// using Dalamud.Game.Text.SeStringHandling;
// using Dalamud.Game.Text.SeStringHandling.Payloads;
// using Dalamud.Interface.ImGuiNotification;
// using PomanderoSplit.Utils;

// namespace PomanderoSplit;

// public class LiveSplitClient : IDisposable
// {
//     private Socket? SocketClient { get; set; }
//     private NamedPipeClientStream? pipeClientStream { get; set; }

//     public bool UseTCP { get; set; }
//     public int Port { get; private set; }

//     private const int ConnectionAttempts = 3;

//     public bool resetSplits = true;
//     public bool onRun = false;
//     public bool deepDungeonEnd = false;

//     public LiveSplitClient(Plugin plugin)
//     {
//         UseTCP = plugin.Configuration.UseTCP;
//         Port = plugin.Configuration.LivesplitPort;
//         if (plugin.Configuration.Connect)
//         {
//             Connect();
//         }
//     }

//     internal void ChangePort(int Value)
//     {
//         Port = Value;
//         Reconnect();
//     }

//     internal void ChangeTCP(bool value)
//     {
//         UseTCP = value;
//         if (!UseTCP)
//         {
//             Disconnect();
//         }
//     }

//     public bool Status()
//     {
//         if (UseTCP)
//         {
//             return SocketClient?.Connected ?? false;
//         }
//         else
//         {
//             return pipeClientStream?.IsConnected ?? false;
//         }
//     }

//     public void Connect()
//     {
//         if (Status())
//         {
//             Dalamud.Log.Warning("Already connected.");
//             return;
//         }

//         // use named pipe
//         if (!UseTCP)
//         {
//             try
//             {
//                 pipeClientStream?.Dispose();
//                 pipeClientStream = new(".", "LiveSplit", PipeDirection.InOut, PipeOptions.WriteThrough);
//                 pipeClientStream?.Connect(TimeSpan.FromSeconds(5));
//             }
//             catch (Exception ex)
//             {
//                 Dalamud.Log.Error($"Connect error: {ex}\n --- Please ensure that LiveSplit is running ---");
//                 pipeClientStream?.Dispose();
//                 pipeClientStream = null;
//             }
//         }
//         else
//         {
//             for (var attempt = 0; attempt != ConnectionAttempts; ++attempt)
//             {
//                 try
//                 {
//                     SocketClient?.Dispose(); // redundancy
//                     SocketClient = new(SocketType.Stream, ProtocolType.Tcp);
//                     SocketClient.Connect("localhost", Port);
//                 }
//                 catch (Exception ex)
//                 {
//                     Dalamud.Log.Error($"Connect error: {ex}");
//                     SocketClient?.Dispose();
//                     SocketClient = null;
//                 }

//                 // TODO: Add delay betwen attempts

//                 if (Status())
//                 {
//                     Dalamud.Log.Info("Connection was successful.");
//                     LogHelper.ReportSuccess("Connection was successful.");
//                     return;
//                 }

//                 SocketClient?.Dispose();
//                 SocketClient = null;

//                 Dalamud.Log.Info($"Connection to port \'{Port}\', attempt {attempt}/{ConnectionAttempts}");
//             }

//             Dalamud.Log.Warning($"Failed to connect after {ConnectionAttempts} attempts");
//             LogHelper.ReportFailure($"Failed to connect");
//         }
//     }

//     public void Disconnect()
//     {
//         if (!Status())
//         {
//             Dalamud.Log.Debug("LiveSplit connection is already off.");
//             SocketClient?.Dispose();
//             SocketClient = null;
//             return;
//         }

//         try
//         {
//             SocketClient?.Shutdown(SocketShutdown.Both);
//             SocketClient?.Close();
//             SocketClient?.Disconnect(false);

//             Dalamud.Log.Info("Closed connection.");
//             LogHelper.ReportSuccess("Closed connections.");
//         }
//         catch (Exception ex)
//         {
//             Dalamud.Log.Error($"Disconnection error: {ex}");
//         }
//         finally
//         {
//             pipeClientStream?.Dispose();
//             SocketClient?.Dispose();
//             SocketClient = null;
//         }
//     }

//     public void Reconnect()
//     {
//         Disconnect();
//         Connect();
//     }

//     private void SendMessage(string message)
//     {
//         if (!Status())
//         {
//             Dalamud.Log.Debug("SendMessage was requested while not connected.");
//             return;
//         }

//         var formatted = message;
//         if (!formatted.EndsWith('\n')) formatted += Environment.NewLine;

//         var encoded = Encoding.ASCII.GetBytes(formatted);

//         try
//         {
//             Dalamud.Log.Debug($"Sending message: {message} | {encoded}");
//             if (!UseTCP && pipeClientStream != null)
//             {
//                 pipeClientStream.Write(encoded);
//                 pipeClientStream.Flush();
//             }
//             else
//             {
                
//                 SocketClient?.Send(encoded);
//             }
//         }
//         catch (Exception ex)
//         {
//             Dalamud.Log.Error($"SendMessage error: {ex}");
//             Disconnect();
//         }
//     }

//     public void Dispose()
//     {
//         SocketClient?.Dispose();
//         pipeClientStream?.Dispose();
//     }

//     public void TryReset()
//     {
//         deepDungeonEnd = false;
//         if (resetSplits)
//         {
//             resetSplits = false;
//             Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload("Reset!"), new UIForegroundPayload(0)));
//             Reset();
//         }
//     }

//     public void Reset()
//     {
//         resetSplits = false;
//         onRun = false;
//         deepDungeonEnd = false;
//         SendMessage("reset");
//     }

//     public void StartOrSplit() => SendMessage("startorsplit");

//     public void Start() => SendMessage("play");

//     public void Resume() => SendMessage("resume");

//     public void Split() => SendMessage("split");

//     public void Pause() => SendMessage("pause");
//     public void PauseQueueEnd()
//     {
//         resetSplits = true;
//         SendMessage("pause");
//     }
// }