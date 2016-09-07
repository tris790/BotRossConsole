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
using System.Collections.Generic;

namespace BotRoss
{
    public class BotRossClient
    {
        private DiscordClient _client;
        private bool _Playing = false;
        private bool _Audio = true;

        private void Start(string[] args)
        {
            Console.Title = "Bot Ross - Admin Pannel";

#if PRIVATE
            PrivateModules.Install(_client);
#endif

            if (File.Exists("Ascii.txt"))
            {
                string ascii = File.ReadAllText("Ascii.txt");
                Console.WriteLine(ascii);
            }
            _client = new DiscordClient();

            Console.ForegroundColor = ConsoleColor.Green;

            _client.Log.Message += (s, e) => Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}");

            _client.MessageReceived += _client_MessageReceived;

            _client.ExecuteAndWait(async () =>
            {
                while (true)
                    try
                    {
                        await _client.Connect("00000000000000000"); //replace with your own
                        break;
                    }
                    catch (Exception ex)
                    {
                        _client.Log.Error($"Couldn't log in", ex);
                        await Task.Delay(_client.Config.FailedReconnectDelay);
                    }
            });

            Console.ResetColor();
        }

        public void Commands(MessageEventArgs e)
        {
            if (e.Message.Text.ToLower() == "!gtfo")
            {
                _client.Disconnect();
                _client.Dispose();
            }
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
        }

        public async void SendAudio(string filePath, MessageEventArgs e)
        {
            try
            {
                if (_Audio == true)
                    _client.UsingAudio(x =>
                    {
                        x.Mode = AudioMode.Outgoing;
                    });
                _Audio = false;
                _Playing = true;
                var voiceChannel = e.Message.Server.VoiceChannels.FirstOrDefault();
                if (e.Message.User.VoiceChannel != null)
                    voiceChannel = e.Message.User.VoiceChannel;
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

                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0 && _Playing == true)
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

        public async void LeaveVoiceChannel(MessageEventArgs e)
        {
            try
            {
                var voiceChannel = e.Message.Server.VoiceChannels.FirstOrDefault();
                if (e.Message.User.VoiceChannel != null)
                    voiceChannel = e.Message.User.VoiceChannel;
                await voiceChannel.LeaveAudio();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now} - Exception: {ex.Message}");
                Console.ResetColor();
            }

        }

        private void _client_MessageReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now} - {e.Message.User}: {e.Message.Text}");
            if (e.Message.Text.StartsWith("!"))
                Commands(e);
        }
    }
}
