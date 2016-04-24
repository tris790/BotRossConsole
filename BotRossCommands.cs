using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using DiscordBot;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotRoss
{
    static public class BotRossCommands
    {
        static public void CreateCommands(DiscordClient _client)
        {
            //TEMPLATE
            _client.GetService<CommandService>().CreateCommand("welcome")
            .Alias(new string[] { "hi", "hey" })
            .Description("Greets the user.")
            .Parameter("GreetedPerson", ParameterType.Required)
            .Do(async e =>
            {
                await e.Channel.SendMessage($"{e.User.Name} greets {e.GetArg("GreetedPerson")}");
            });

            #region Utility

            _client.GetService<CommandService>().CreateCommand("botinfo")
           .Alias(new string[] { "information" })
           .Description("Gives information about the bot.")
           .Do(async e =>
           {
               await e.Channel.SendMessage($"```**Memory Usage:** {Environment.WorkingSet.ToReadableMemory()}\n**Uptime:** {(DateTime.Now - Process.GetCurrentProcess().StartTime).ToReadableString()}```");
           });

            //DELETE MESSAGE
            _client.GetService<CommandService>().CreateCommand("deletemessage")
           .Alias(new string[] { "dm" })
           .Description("Delete's a or several messages.")
           .Parameter("number", ParameterType.Optional)
           .Do(async e =>
           {
               if (PermissionResolver(e.User, e.Channel) == (int)PermissionLevel.BotOwner)
               {
                   int number = 1;
                   if (e.GetArg("number") != "")
                   {
                       int.TryParse(e.GetArg("number"), out number);
                       if (number > 100)
                           number = 100;
                   }
                   await e.Channel.DownloadMessages(number, null, Relative.Before, true);
                   foreach (var message in e.Channel.Messages)
                       await message.Delete();
               }
           });
            //CLOSE APPLICATION
            _client.GetService<CommandService>().CreateCommand("close")
            .Alias(new string[] { "exit", "gtfo" })
            .Description("Disconnects the bot and close the program.")
            .Do(async e =>
            {
                await _client.Disconnect();
                _client.Dispose();
                Environment.Exit(0);
            });

            //USERLIST
            _client.GetService<CommandService>().CreateCommand("userlist")
            .Alias(new string[] { "ul" })
            .Description("Gets the list of user in the current server.")
            .Parameter("all", ParameterType.Optional)
            .Do(async e =>
            {
                if (e.GetArg("all") != null)
                {
                    string message = "";
                    foreach (var channel in e.Server.AllChannels)
                    {
                        message += $"[{channel.Name.ToUpper()}]\n";
                        foreach (var user in channel.Users)
                        {
                            message += $"\t{user}\n";
                        }
                        message += "\n";
                    }
                    if (message != "")
                        await e.Channel.SendMessage($"```{message}```");
                }
            });

            //INFO USER
            _client.GetService<CommandService>().CreateCommand("info")
            .Alias(new string[] { "i" })
            .Description("Get's Bob Ross infomation.")
            .Parameter("user", ParameterType.Required)
            .Parameter("discriminator", ParameterType.Required)
            .Do(async e =>
            {
                var user = _client.FindUser(e, e.GetArg(0), e.GetArg(1));
                await e.Channel.SendMessage($"```xl\nUser: {user.Result.Name}\nID: {user.Result.Id}\nStatus: {user.Result.Status}\nVoice Channel: {user.Result.VoiceChannel}\nCurrent Game: {user.Result.CurrentGame}\nAvatar: {user.Result.AvatarUrl}\nLast Activity: {user.Result.LastActivityAt}\nLast Online: {user.Result.LastOnlineAt}```");
            });

            //HELP COMMANDS
            _client.GetService<CommandService>().CreateCommand("commands")
            .Alias(new string[] { "command", "helpme" })
            .Description("Get's all current commands.")
            .Do(async e =>
            {
                string message = "";
                var req = from command in _client.GetService<CommandService>().AllCommands
                          orderby command.Text
                          select command;

                foreach (var command in req)
                {
                    message += $"**{command.Text}:**";
                    foreach (var alias in command.Aliases)
                        message += $" {alias}";
                    message += $"\n";
                }

                await e.Channel.SendMessage($"{message}");
            });

            //STOP AUDIO
            _client.GetService<CommandService>().CreateCommand("stopaudio")
            .Alias(new string[] { "stop", "leave" })
            .Description("Stop's the current song.")
            .Do(async e =>
            {
                var voiceChannel = e.Server.VoiceChannels.FirstOrDefault();
                if (e.User.VoiceChannel != null)
                    voiceChannel = e.User.VoiceChannel;
                await voiceChannel.LeaveAudio();
            });

            //CHANGE AVATAR
            _client.GetService<CommandService>().CreateCommand("changeavatar")
            .Alias(new string[] { "avatar" })
            .Description("Change's Bot Ross avatar picture to avatar.png.")
            .Do(async e =>
            {
                Stream pic = new FileStream("avatar.Jpeg", FileMode.Open);
                await _client.CurrentUser.Edit(avatar: pic, avatarType: ImageType.Jpeg);
            });

            //CHANGE NAME
            _client.GetService<CommandService>().CreateCommand("changegame")
            .Alias(new string[] { "game" })
            .Description("Change's Bot Ross avatar picture to avatar.png.")
            .Parameter("Game", ParameterType.Required)
            .Do(e =>
            {
                _client.SetGame(e.GetArg("Game"));
            });
            #endregion

            #region VoiceMemes

            _client.GetService<CommandService>().CreateCommand("johncena")
            .Alias(new string[] { "cena", "john" })
            .Description("Plays John Cena.")
            .Do(async e =>
            {
                await SendAudio("cena.mp3", e.Message);
            });

            _client.GetService<CommandService>().CreateCommand("suckyd")
            .Alias(new string[] { "suck", "d" })
            .Description("Plays a sucking sound.")
            .Do(async e =>
            {
                await SendAudio("suckyd.mp3", e.Message);
            });

            _client.GetService<CommandService>().CreateCommand("bobross")
            .Alias(new string[] { "bobross", "ross", "whoareyou" })
            .Description("Plays Bob Ross.")
            .Do(async e =>
            {
                await SendAudio("bobross.mp3", e.Message);
            });

            _client.GetService<CommandService>().CreateCommand("allahuakbar")
            .Alias(new string[] { "allah", "allahkbar", "boom" })
            .Description("Plays Allahu Akbar.")
            .Do(async e =>
            {
                await SendAudio("allahuakbar.mp3", e.Message);
            });

            _client.GetService<CommandService>().CreateCommand("bloup")
            .Description("Bloup Bloup.")
            .Do(async e =>
            {
                await SendAudio("bloup.mp3", e.Message);
            });

            _client.GetService<CommandService>().CreateCommand("cry")
            .Alias(new string[] { "sad" })
            .Description("Cries for you.")
            .Do(async e =>
            {
                await SendAudio("cry.mp3", e.Message);
            });

            _client.GetService<CommandService>().CreateCommand("donaldtrump")
            .Alias(new string[] { "trump", "donald" })
            .Description("Plays a small loan of a million dollars.")
            .Do(async e =>
            {
                await SendAudio("donaldtrump.mp3", e.Message);
            });

            _client.GetService<CommandService>().CreateCommand("cancer")
            .Alias(new string[] { "tumor" })
            .Description("CANCER.")
            .Do(async e =>
            {
                await SendAudio("cancer.mp3", e.Message);
            });

            _client.GetService<CommandService>().CreateCommand("cumming")
            .Alias(new string[] { "cum", "cumming" })
            .Description("I'm fucking cumming.")
            .Do(async e =>
            {
                await SendAudio("cuming.mp3", e.Message);
            });
            #endregion

            #region Fun/Dev
            _client.GetService<CommandService>().CreateCommand("question")
            .Alias(new string[] { "ask" })
            .Description("Ask's Bot Ross a question.")
            .Parameter("Question", ParameterType.Multiple)
            .Do(async e =>
            {
                Random rnd = new Random();
                string answer = "";
                switch (rnd.Next(0, 6))
                {
                    case 0:
                    case 1:
                        answer = "Yes";
                        break;
                    case 2:
                    case 3:
                        answer = "No!";
                        break;
                    case 4:
                        answer = "Maybe";
                        break;
                    default:
                        answer = "I don't fucking care bro";
                        break;
                }
                await e.Channel.SendMessage($"{answer}");
            });           
            #endregion
        }
        static public async Task
        SendAudio(string filePath, Message e)
        {
            try
            {
                var voiceChannel = e.Server.VoiceChannels.FirstOrDefault();
                if (e.User.VoiceChannel != null)
                    voiceChannel = e.User.VoiceChannel;
                var _vClient = await voiceChannel.JoinAudio();
                var channelCount = 2;
                var OutFormat = new WaveFormat(48000, 16, channelCount);
                using (var MP3Reader = new Mp3FileReader("Audio/" + filePath))
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat))
                {
                    resampler.ResamplerQuality = 60;
                    int blockSize = OutFormat.AverageBytesPerSecond / 50;
                    byte[] buffer = new byte[blockSize];
                    int byteCount;

                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                    {
                        if (byteCount < blockSize)
                        {
                            // Incomplete Frame
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }
                        _vClient.Send(buffer, 0, blockSize);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now} - Exception: {ex.Message}");
                Console.ResetColor();
            }
        }
        static public int PermissionResolver(User user, Channel channel)
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

