using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;

namespace PomanderoSplit;

public class CommandHandler : IDisposable
{
    private Commands commands;
    private ICommandManager manager;
    public IChatGui chatGui;
    internal static List<KeyValuePair<List<string>, Action<string[]>>> command_all = new List<KeyValuePair<List<string>, Action<string[]>>>();

    public CommandHandler(Plugin plugin, ICommandManager manager, IChatGui chatGui)
    {
        commands = new Commands(plugin, chatGui, command_all);
        this.manager = manager;
        this.chatGui = chatGui;
        
        command_all.Add(new KeyValuePair<List<string>, Action<string[]>>(Commands.command_settings, (string[] args) => commands.OnSettings(args)));
        command_all.Add(new KeyValuePair<List<string>, Action<string[]>>(Commands.command_help, (string[] args) => commands.OnHelp()));

        manager.AddHandler(Commands.CommandName[1], new CommandInfo((string command, string args) => commands.OnCommand(args))
        {
            HelpMessage = $"Alias for '{Commands.CommandName[0]}'."
        });

        manager.AddHandler(Commands.CommandName[0], new CommandInfo((string command, string args) => commands.OnCommand(args))
        {
            HelpMessage = $"Opens the main window of the plugin.\n\t More info with help."
        });
    }
    public void Dispose()
    {
        foreach (var command in manager.Commands)
        {
            manager.RemoveHandler(command.Key);
        }

        command_all.Clear();
        manager = null!;
        chatGui = null!;
    } 
}

internal class Commands
{
    private Plugin plugin;
    private IChatGui chatGui;
    public List<KeyValuePair<List<string>, Action<string[]>>> command_all;
    internal Commands(Plugin plugin, IChatGui chatGui, List<KeyValuePair<List<string>, Action<string[]>>> command_all)
    {
        this.plugin = plugin;
        this.chatGui = chatGui;
        this.command_all = command_all;
    }

    internal static ImmutableArray<string> CommandName = ["/pomandero", "/splits"];

    internal static List<string> command_help = ["help", "h"];
    
    internal static List<string> command_settings = ["settings", "config", "cfg"];
    
    internal void OnCommand(string args)
    {
        List<string> split_args = args.Split(' ').ToList();

        var matchedCommand = command_all.FirstOrDefault(kv => kv.Key.Contains<string>(split_args[0].ToString()));

        Action handler = matchedCommand.Key switch
        {
            List<string> key when key != null => () => matchedCommand.Value?.Invoke(split_args.ToArray()),
            _ => () =>
            {
                var text = $"Invalid command \'{split_args[0]}\' consult \'{CommandName[0]} help\'.";
                chatGui.Print(new SeString(new UIForegroundPayload(539), new TextPayload(text), new UIForegroundPayload(0)));
            }
        };
        handler();
    }


    internal void OnSettings(string[] args)
    {
        plugin.ToggleConfigUI();
    }

    internal void OnDefault()
    {
        plugin.ToggleMainUI();
    }

    internal void OnHelp()
    {
        plugin.CommandHandler.chatGui.Print("\n[ /splits, /pomandero ] [ settings, config, cfg ] opens the settings window.\n\n Help function is WIP.\n");
    }

}