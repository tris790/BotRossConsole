using Discord;
using Discord.Audio;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Discord.Commands;
using Discord.Modules;
using DiscordBot.Modules.Admin;
using DiscordBot.Modules.Colors;
using DiscordBot.Modules.Feeds;
using DiscordBot.Modules.Github;
using DiscordBot.Modules.Modules;
using DiscordBot.Modules.Public;
using DiscordBot.Modules.Status;
using DiscordBot.Modules.Twitch;
using DiscordBot.Services;
using DiscordBot;
using Discord.Commands.Permissions.Levels;
using System.Text;
using System.Collections.Generic;

namespace BotRoss
{
    public class Program
    {

        private DiscordClient _client;
        public const string clientId = "00000000000000000"; //replace with your own
        public static void Main(string[] args) => new Program().Start(args);

        private void Start(string[] args)
        {
            GlobalSettings.Users.DevId = 00000000000000000; //replace with your own
            #region AsciiLogo
            if (File.Exists("Ascii.txt"))
            {
                string ascii = File.ReadAllText("Ascii.txt");
                Console.WriteLine(ascii);
            }
            #endregion
            Console.Title = "Bot Ross - Admin Console";
            _client = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Verbose;
                x.LogHandler = OnLogMessage;
            }
            );

            #region Commands
            _client.UsingCommands(x => {
                x.PrefixChar = '!';
                x.HelpMode = HelpMode.Public;
                x.ExecuteHandler += OnCommandExecuted;
                x.ErrorHandler += OnCommandError;
            });
            BotRossCommands.CreateCommands(_client);
            _client.UsingPermissionLevels(PermissionResolver);
            #endregion

            #region Audio
            _client.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
            });
            #endregion

            #region Connection
            //Console.ForegroundColor = ConsoleColor.Green;
            _client.Log.Message += (s, e) => Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}");
            _client.MessageReceived += _client_MessageReceived;
          _client.ExecuteAndWait(async () =>
            {
                while (true)
                    try
                    {
                        await _client.Connect(clientId);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _client.Log.Error($"Couldn't log in", ex);
                        await Task.Delay(_client.Config.FailedReconnectDelay);
                    }
            });
            Console.ResetColor();
            #endregion
        }

        private void OnCommandError(object sender, CommandErrorEventArgs e)
        {
            string msg = e.Exception?.Message;
            if (msg == null) //No exception - show a generic message
            {
                switch (e.ErrorType)
                {
                    case CommandErrorType.Exception:
                        msg = "Unknown error.";
                        break;
                    case CommandErrorType.BadPermissions:
                        msg = "You do not have permission to run this command.";
                        break;
                    case CommandErrorType.BadArgCount:
                        msg = "You provided the incorrect number of arguments for this command.";
                        break;
                    case CommandErrorType.InvalidInput:
                        msg = "Unable to parse your command, please check your input.";
                        break;
                    case CommandErrorType.UnknownCommand:
                        msg = "Unknown command.";
                        break;
                }
            }
            if (msg != null)
            {
                e.Channel.SendMessage($"Error: {msg}");
                _client.Log.Error("Command", msg);
            }
        }

        private void OnCommandExecuted(object sender, CommandEventArgs e)
        {
            _client.Log.Info("Command", $"{e.Command.Text} ({e.User.Name})");           
        }

        private void OnLogMessage(object sender, LogMessageEventArgs e)
        {
            //Color
            ConsoleColor color;
            switch (e.Severity)
            {
                case LogSeverity.Error: color = ConsoleColor.Red; break;
                case LogSeverity.Warning: color = ConsoleColor.Yellow; break;
                case LogSeverity.Info: color = ConsoleColor.Green; break;
                case LogSeverity.Verbose: color = ConsoleColor.Gray; break;
                case LogSeverity.Debug: default: color = ConsoleColor.Cyan; break;
            }

            //Exception
            string exMessage;
            Exception ex = e.Exception;
            if (ex != null)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                exMessage = ex.Message;
            }
            else
                exMessage = null;

            //Source
            string sourceName = e.Source?.ToString();

            //Text
            string text;
            if (e.Message == null)
            {
                text = exMessage ?? "";
                exMessage = null;
            }
            else
                text = e.Message;

            //Build message
            StringBuilder builder = new StringBuilder(text.Length + (sourceName?.Length ?? 0) + (exMessage?.Length ?? 0) + 5);
            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }
            for (int i = 0; i < text.Length; i++)
            {
                //Strip control chars
                char c = text[i];
                if (!char.IsControl(c))
                    builder.Append(c);
            }
            if (exMessage != null)
            {
                builder.Append(": ");
                builder.Append(exMessage);
            }

            text = builder.ToString();
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }

        private void _client_MessageReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now} - {e.Message.User}: {e.Message.Text}");
            if (e.Message.Text.Contains("@Barack Obama Ross"))
                Console.Beep();
        }

        private int PermissionResolver(User user, Channel channel)
        {
            if (user.Id == GlobalSettings.Users.DevId)
                return (int)PermissionLevel.BotOwner;
            if (user.Server != null)
            {
                if (user == channel.Server.Owner)
                    return (int)PermissionLevel.ServerOwner;
                
                var serverPerms = user.ServerPermissions;
                if (serverPerms.ManageRoles)
                    return (int)PermissionLevel.ServerAdmin;
                if (serverPerms.ManageMessages && serverPerms.KickMembers && serverPerms.BanMembers)
                    return (int)PermissionLevel.ServerModerator;

                var channelPerms = user.GetPermissions(channel);
                if (channelPerms.ManagePermissions)
                    return (int)PermissionLevel.ChannelAdmin;
                if (channelPerms.ManageMessages)
                    return (int)PermissionLevel.ChannelModerator;
            }
            return (int)PermissionLevel.User;
        }
    }
    
}
