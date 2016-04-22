using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
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
            _client.GetService<CommandService>().CreateCommand("Fuck you")
            .Alias(new string[] { "t", "fu" })
            .Description("Fuck's the user.")
            .Parameter("GreetedPerson", ParameterType.Required)
            .Do(async e =>
            {
                await e.Channel.SendMessage($"{e.User.Name} greets {e.GetArg("GreetedPerson")}");
            });

            //GTFO
            _client.GetService<CommandService>().CreateCommand("close")
            .Alias(new string[] { "exit", "gtfo" })
            .Description("Disconnects the bot and close the program.")
            .Do(async e =>
            {
                await _client.Disconnect();
                _client.Dispose();
                Environment.Exit(0);
            });

            _client.GetService<CommandService>().CreateCommand("userlist")
            .Alias(new string[] { "ul" })
            .Description("Gets the list of user in the current server.")
            .Parameter("all", ParameterType.Optional)
            .Do(async e =>
            {
                if (e.GetArg("all") != null)
                {
                    string Message = "";
                    foreach (var channel in e.Server.AllChannels)
                    {
                        Message += $"[{channel.Name.ToUpper()}]\n";
                        foreach (var user in channel.Users)
                        {
                            Message += $"\t{user}\n";
                        }
                        Message += "\n";
                    }
                    if (Message != "")
                        await e.Channel.SendMessage($"```{Message}```");
                }
            });



            /*
            else if (e.Message.Text.ToLower() == "!userlist")
            {
                string userlist = "";
                foreach (var user in e.Channel.Users)
                {
                    userlist += $"{user}\n";
                };
                e.Channel.SendMessage(userlist);
            }
            else if (e.Message.Text.ToLower() == "!cena")
                SendAudio("cena.mp3", e);
            else if (e.Message.Text.ToLower() == "!suckyd")
                SendAudio("suckyd.mp3", e);
            else if (e.Message.Text.ToLower() == "!stop")
                _Playing = false;
            else if (e.Message.Text.ToLower() == "!bobross")
                SendAudio("bobross.mp3", e);
            else if (e.Message.Text.ToLower() == "!allahuakbar")
                SendAudio("allahuakbar.mp3", e);
            else if (e.Message.Text.ToLower() == "!cry")
                SendAudio("cry.mp3", e);
            else if (e.Message.Text.ToLower() == "!donaldtrump")
                SendAudio("donaldtrump.mp3", e);
            else if (e.Message.Text.ToLower() == "!cancer")
                SendAudio("cancer.mp3", e);
            else if (e.Message.Text.ToLower() == "!bloup")
                SendAudio("bloup.mp3", e);
            else if (e.Message.Text.ToLower() == "!cuming")
                SendAudio("cuming.mp3", e);
            else if (e.Message.Text.ToLower() == "!avatar")
            {
                Stream pic = new FileStream("avatar.Jpeg", FileMode.Open);
                _client.CurrentUser.Edit(avatar: pic, avatarType: ImageType.Jpeg);
            }
            else if (e.Message.Text.ToLower() == "!leave")
                LeaveVoiceChannel(e);
            else if (e.Message.Text.StartsWith("!game".ToLower()))
            {
                if (e.Message.Text.Length > 5)
                    _client.SetGame(e.Message.Text.Substring(5));
            }
            else if (e.Message.Text.ToLower() == "!memory")
                e.Channel.SendMessage($"The bot is using: {Process.GetCurrentProcess().WorkingSet64.ToReadableMemory()}.");
            else if (e.Message.Text.ToLower() == "!uptime")
                e.Channel.SendMessage($"The bot has been up for {(DateTime.Now - Process.GetCurrentProcess().StartTime).ToReadableString()}.");
            else if (e.Message.Text.ToLower() == "!commands")
            {
                e.Channel.SendMessage($"gtfo, cena, stop, bobross, allahuakbar, cry, donaldtrump, cancer, bloup, avatar, leave, game, uptime, commands");
            }
            else if (e.Message.Text.ToLower() == "!airhorn")
                return;



            else if (e.Message.Text.StartsWith("!"))
                e.Channel.SendMessage("Command not found");
             */
        }
    }
}
