

/*
    'List' or 'Dictionary' are dynamic objects since commands wont be added dynamic they are not needed.
    Things here can be sinplified alot using an switch case statement making it more performatic and less cucumbersome they also make alias simpler to handle.
*/


using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace PomanderoSplit;

public class CommandHandler : IDisposable
{
    private Plugin Plugin { get; init; }

    private Commands commands;

    public static List<KeyValuePair<List<string>, Action<string[]>>> command_all = [];

    public CommandHandler(Plugin plugin)
    {
        Plugin = plugin;

        commands = new Commands(plugin, command_all);

        command_all.Add(new KeyValuePair<List<string>, Action<string[]>>(Commands.command_default, (string[] args) => commands.OnDefault()));
        command_all.Add(new KeyValuePair<List<string>, Action<string[]>>(Commands.command_settings, (string[] args) => commands.OnSettings(args)));
        command_all.Add(new KeyValuePair<List<string>, Action<string[]>>(Commands.command_help, (string[] args) => commands.OnHelp()));

        Dalamud.Commands.AddHandler(Commands.CommandName[1], new CommandInfo((string command, string args) => commands.OnCommand(args))
        {
            HelpMessage = $"Alias for '{Commands.CommandName[0]}'."
        });

        Dalamud.Commands.AddHandler(Commands.CommandName[0], new CommandInfo((string command, string args) => commands.OnCommand(args))
        {
            HelpMessage = $"Opens the main window of the plugin.\n\t More info with help."
        });
    }

    public void Dispose()
    {
        foreach (var command in Dalamud.Commands.Commands)
        {
            Dalamud.Commands.RemoveHandler(command.Key);
        }

        command_all.Clear();
    }
}

public class Commands
{
    private Plugin Plugin { get; init; }

    public List<KeyValuePair<List<string>, Action<string[]>>> command_all;

    public Commands(Plugin plugin, List<KeyValuePair<List<string>, Action<string[]>>> command_all)
    {
        Plugin = plugin;
        this.command_all = command_all;
    }

    public static ImmutableArray<string> CommandName = ["/pomandero", "/splits"];

    public static List<string> command_default = [""];

    public static List<string> command_help = ["help", "h"];

    public static List<string> command_settings = ["settings", "config", "cfg"];

    public void OnCommand(string args)
    {
        var split_args = args.Split(' ').ToList();

        var matchedCommand = command_all.FirstOrDefault(kv => kv.Key.Contains<string>(split_args[0].ToString()));

        Action handler = matchedCommand.Key switch
        {
            List<string> key when key != null => () => matchedCommand.Value?.Invoke([.. split_args]),
            _ => () =>
            {
                var text = $"Invalid command \'{split_args[0]}\' consult \'{CommandName[0]} help\'.";
                Dalamud.Chat.Print(new SeString(new UIForegroundPayload(539), new TextPayload(text), new UIForegroundPayload(0)));
            }
        };
        handler();
    }

    public void OnSettings(string[] args)
    {
        Plugin.ConfigWindow.Toggle();
    }

    public void OnDefault()
    {
        Plugin.MainWindow.Toggle();
    }

    public void OnHelp()
    {
        Dalamud.Chat.Print("\n[ /splits, /pomandero ] [ settings, config, cfg ] opens the settings window.\n\n Help function is WIP.\n");
    }
}



// using System;
// using System.Text;
// using Dalamud.Game.Command;
// using Dalamud.Game.Text.SeStringHandling;
// using Dalamud.Game.Text.SeStringHandling.Payloads;

// namespace PomanderoSplit;

// public class CommandHandler : IDisposable
// {
//     private Plugin Plugin { get; init; }

//     public CommandHandler(Plugin plugin)
//     {
//         Plugin = plugin;

//         Dalamud.Commands.AddHandler("/pomandero", new CommandInfo((string command, string args) => OnCommand(args))
//         {
//             HelpMessage = $"Opens the main window of the plugin.\n\t More info with help."
//         });
//         Dalamud.Commands.AddHandler("/splits", new CommandInfo((string command, string args) => OnCommand(args))
//         {
//             HelpMessage = $"Alias for '/pomandero'."
//         });
//     }

//     public void OnCommand(string arg)
//     {
//         string[] args = [.. arg.Split(' ')];
//         Action handler = args[0] switch
//         {
//             "" when args.Length == 1 => () => { Plugin.MainWindow.IsOpen = true; },
            
//             "config" when args.Length == 1 => Plugin.ConfigWindow.Toggle,
//             "cfg" when args.Length == 1 => Plugin.ConfigWindow.Toggle,
//             "settings" when args.Length == 1 => Plugin.ConfigWindow.Toggle,

//             "help" when args.Length == 1 => HelpMessage,
//             "h" when args.Length == 1 => HelpMessage,

//             "connect" when args.Length == 1 => Plugin.Connection.Connect,
//             "disconnect" when args.Length == 1 => Plugin.Connection.Disconnect,
//             "reconnect" when args.Length == 1 => Plugin.Connection.Reconnect,
//             "status" when args.Length == 1 => () => { Dalamud.Chat.Print($"{Plugin.Connection.Status}"); },

//             _ => () =>
//             {
//                 var text = $"Invalid command \'{args[0]}\' consult \'pomandero help\'.";
//                 Dalamud.Chat.Print(new SeString(new UIForegroundPayload(539), new TextPayload(text), new UIForegroundPayload(0)));
//             }
//         };
//         handler();
//     }

//     private static void HelpMessage()
//     {
//         StringBuilder Message = new();
//         void helpMessageBuilder(string arg, string help) => Message.Append($"pomandero{(arg != "" ? " " : "")}{arg} -> {help}\n");

//         helpMessageBuilder("", "Display pomandero main window.");
//         helpMessageBuilder("settings", "pomandero tweaks window.");
//         helpMessageBuilder("connect", "Connect.");
//         helpMessageBuilder("disconnect", "Diconnect.");
//         helpMessageBuilder("reconnect", "Reconnect.");
//         helpMessageBuilder("status", "Connection status.");

//         helpMessageBuilder("help", "This help message");

//         Message.Remove(Message.Length - 1, 1);

//         Dalamud.Chat.Print(Message.ToString());
//     }

//     public void Dispose()
//     {
//         Dalamud.Commands.RemoveHandler("/pomandero");
//         Dalamud.Commands.RemoveHandler("/splits");
//     }
// }