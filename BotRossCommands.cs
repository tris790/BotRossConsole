using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordBot;
using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using Google.Apis.Urlshortener.v1.Data;
using Google.Apis.Urlshortener.v1;
using System.Speech.AudioFormat;
using RiotApi.Net.RestClient;
using RiotApi.Net.RestClient.Configuration;
using System.Collections.Generic;
using RiotApi.Net.RestClient.Dto.League;


namespace BotRoss
{
    static public class BotRossCommands
    {
        public const string googleApiKey = "00000000000000000"; //put your own
        public const string searchEngineId = "00000000000000000"; //put your own
        public const string lolKey = "00000000000000000"; //put your own
        public const string lolApi = "00000000000000000"; //put your own
        public const string applicationPath = "BotRoss.exe";
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

            _client.GetService<CommandService>().CreateCommand("restart")
            .Description("Restart Bot Ross.")
            .Do(async e =>
            {
                if (PermissionResolver(e.User, e.Channel) == (int)PermissionLevel.BotOwner)
                {
                    Process.Start(applicationPath);
                    Environment.Exit(0);
                }
                else
                    await e.Channel.SendMessage($"You do not have the permissions: {PermissionLevel.BotOwner}");
            });

            _client.GetService<CommandService>().CreateCommand("servers")
            .Description("Lists all current server that Bot Ross is a member.")
            .Do(async e =>
            {
                string message = "";
                var req = from server in _client.Servers
                          orderby server.Name
                          select server;
                foreach (var server in req)
                    message += $"**Server:** {server}\n";

                await e.Channel.SendMessage($"{message}");
            });

            _client.GetService<CommandService>().CreateCommand("invitelink")
           .Alias(new string[] { "invlink", "authorizationlink" })
           .Description("Gives the link to authorize Bot Ross to join your server.")
           .Do(async e =>
           {
               await e.Channel.SendMessage($"replace with your own");
           });

            _client.GetService<CommandService>().CreateCommand("botinfo")
           .Alias(new string[] { "information", "infobot" })
           .Description("Gives information about the bot.")
           .Do(async e =>
           {
               User owner = _client.GetUser(e, GlobalSettings.Users.DevId).Result;
               string message = $"**Owner:** {owner.Name} {owner.Discriminator}\n**Memory Usage:** {Environment.WorkingSet.ToReadableMemory()}\n**Uptime:** {(DateTime.Now - Process.GetCurrentProcess().StartTime).ToReadableString()}\n**Number of servers:** {_client.Servers.Count()}";
               await e.Channel.SendMessage($"{message}");
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
                   await e.Channel.DownloadMessages(100, null, Relative.Before, true);
                   var req = (from message in e.Channel.Messages
                              orderby message.Timestamp descending
                              select message).Take(number);
                   foreach (var message in req)
                       await message.Delete();
               }
               else
                   await e.Channel.SendMessage($"You do not have the permissions: {PermissionLevel.BotOwner}");
           });
            //CLOSE APPLICATION
            _client.GetService<CommandService>().CreateCommand("close")
            .Alias(new string[] { "exit", "gtfo" })
            .Description("Disconnects the bot and close the program.")
            .Do(async e =>
            {
                if (PermissionResolver(e.User, e.Channel) == (int)PermissionLevel.BotOwner)
                {
                    await _client.Disconnect();
                    _client.Dispose();
                    Environment.Exit(0);
                }
                else
                    await e.Channel.SendMessage($"You do not have the permissions {PermissionLevel.BotOwner}");
            });

            //USERLIST
            _client.GetService<CommandService>().CreateCommand("userlist")
            .Alias(new string[] { "ul" })
            .Description("Gets the list of user in the current server.")
            .Do(async e =>
            {
                string message = "";
                var req = from channel in e.Server.AllChannels
                          where channel.Name != null && channel.Name != ""
                          select channel;

                foreach (var channel in req)
                {
                    //message += $"[{channel.Name.ToUpper()}]\n";

                    foreach (var user in channel.Users)
                        message += $"\t{user}\n";
                }
                if (message.Length < 2000)
                    await e.Channel.SendMessage($"```\n{message}```");
                else
                    await e.Channel.SendMessage($"Too many members in this server.");
            });

            //INFO USER
            _client.GetService<CommandService>().CreateCommand("info")
            .Alias(new string[] { "i" })
            .Description("Get's a person's infomations.")
            .Parameter("user", ParameterType.Required)
            .Parameter("discriminator", ParameterType.Required)
            .Do(async e =>
            {
                var user = _client.FindUser(e, e.GetArg(0), e.GetArg(1));
                await e.Channel.SendMessage($"**User:** {user.Result.Name}\n**ID:** {user.Result.Id}\n**Status:** {user.Result.Status}\n**Voice Channel:** {user.Result.VoiceChannel}\n**Current Game:** {user.Result.CurrentGame}\n**Avatar:** {user.Result.AvatarUrl}\n**Last Activity:** {user.Result.LastActivityAt}\n**Last Online:** {user.Result.LastOnlineAt}");
            });

            //HELP COMMANDS
            _client.GetService<CommandService>().CreateCommand("commands")
            .Alias(new string[] { "command", "helpme" })
            .Description("Gets all current commands.")
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
            .Description("Stops the current song.")
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
            .Description("Changes Bot Ross avatar picture to avatar.png.")
            .Do(async e =>
            {
                Stream pic = new FileStream("avatar.png", FileMode.Open);
                await _client.CurrentUser.Edit(avatar: pic, avatarType: ImageType.Png);
            });

            //CHANGE Game
            _client.GetService<CommandService>().CreateCommand("changegame")
            .Alias(new string[] { "game" })
            .Description("Changes the game that Bot Ross is currently playing.")
            .Parameter("Game", ParameterType.Unparsed)
            .Do(e =>
            {
                string game = "";
                game = string.Join(" ", e.Args);
                _client.SetGame(game);
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

            _client.GetService<CommandService>().CreateCommand("quenouille")
            .Alias(new string[] { "quebec", "quenouilles" })
            .Description("Plays as-tu vu les belles quenouilles.")
            .Do(async e =>
            {
                await SendAudio("quenouille.mp3", e.Message);
            });

            _client.GetService<CommandService>().CreateCommand("retard")
            .Alias(new string[] { "mind", "outofyourmind", "crazy" })
            .Description("Plays are you out of your mind.")
            .Do(async e =>
            {
                await SendAudio("outmind.mp3", e.Message);
            });

            _client.GetService<CommandService>().CreateCommand("ying")
            .Description("Plays ying from Paladins.")
            .Do(async e =>
            {
                await SendAudio("ying.mp3", e.Message);
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
            .Parameter("Question", ParameterType.Unparsed)
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

            _client.GetService<CommandService>().CreateCommand("image")
            .Alias(new string[] { "imagesearch", "googleimagesearch", "im" })
            .Description("Finds an image based on a keyword.")
            .Parameter("Keyword", ParameterType.Unparsed)
            .Do(async e =>
            {
                if (PermissionResolver(e.User, e.Channel) == (int)PermissionLevel.BotOwner || e.User.Id == 00000000000000000) //replace with your own
                {
                    string keywords = "";
                    keywords = string.Join(" ", e.Args);
                    CustomsearchService customSearchService = new CustomsearchService(new Google.Apis.Services.BaseClientService.Initializer() { ApiKey = googleApiKey });
                    CseResource.ListRequest listRequest = customSearchService.Cse.List(keywords);
                    listRequest.FileType = "png, gif, jpg, bmp";
                    listRequest.Num = 2;
                    listRequest.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;
                    listRequest.Cx = searchEngineId;

                    Search search = listRequest.Execute();
                    await e.Channel.SendMessage($"{search.Items[0].Link}");
                }
            });

            _client.GetService<CommandService>().CreateCommand("url")
            .Alias(new string[] { "shorten", "shortenurl" })
            .Description("Shortens an url.")
            .Parameter("Url", ParameterType.Required)
            .Do(async e =>
            {
                UrlshortenerService service = new UrlshortenerService(new Google.Apis.Services.BaseClientService.Initializer() { ApiKey = googleApiKey });
                Url url = new Url() { LongUrl = e.GetArg("Url") };
                await e.Channel.SendMessage($"{service.Url.Insert(url).Execute().Id}");
            });

            _client.GetService<CommandService>().CreateCommand("speech")
            .Alias(new string[] { "read", "say" })
            .Description("Reads out a message.")
            .Parameter("Keyword", ParameterType.Unparsed)
            .Do(async e =>
            {
                string text = "";
                text = string.Join(" ", e.Args);
                await ReadAudio(text, e.Message);
            });
            #endregion

            #region LeagueOfLegends
            _client.GetService<CommandService>().CreateCommand("lolregion")
            .Alias(new string[] { "region", "regions" })
            .Description("Returns every region available for League of Legends.")
            .Do(async e =>
            {
                string regions = "";
                int index = 0;
                foreach (var region in Enum.GetValues(typeof(RiotApiConfig.Regions)))
                {
                    regions += $"([{index}]{region}) ";
                    index++;
                }
                await e.Channel.SendMessage($"{regions}");
            });

            _client.GetService<CommandService>().CreateCommand("lolplatforms")
            .Alias(new string[] { "platform", "platforms" })
            .Description("Returns every plaform available for League of Legends.")
            .Do(async e =>
            {
                string platforms = "";
                int index = 0;
                foreach (var platform in Enum.GetValues(typeof(RiotApiConfig.Platforms)))
                {
                    platforms += $"([{index}]{platform}) ";
                    index++;
                }
                await e.Channel.SendMessage($"{platforms}");
            });

            _client.GetService<CommandService>().CreateCommand("leagueprofile")
            .Alias(new string[] { "lolprofile", "lolinfo" })
            .Description("Get's a league of legends player information.")
            .Parameter("region", ParameterType.Required)
            .Parameter("platform", ParameterType.Required)
            .Parameter("name", ParameterType.Unparsed)

            .Do(async e =>
            {
                IRiotClient riotClient = new RiotClient(lolApi);
                string name = e.GetArg("name").ToLower().RemoveWhitespace();
                var summoner = riotClient.Summoner.GetSummonersByName((RiotApiConfig.Regions)int.Parse(e.Args[0]), name);
                var player = summoner[name];
                string rank = "";
                try
                {
                    string listParticipants = "";
                    string runes = "";
                    long[] aplayers = new long[1];
                    aplayers[0] = player.Id;
                    string division = "";
                    var AllRunes = riotClient.LolStaticData.GetRuneList(RiotApiConfig.Regions.NA, runeListData: "all");
                    Dictionary<string, IEnumerable<LeagueDto>> summonerLeagues = riotClient.League.GetSummonerLeagueEntriesByIds(RiotApiConfig.Regions.NA, aplayers);
                    foreach (var value in summonerLeagues.Values.FirstOrDefault())
                    {
                        foreach (var entry in value.Entries)
                            division = entry.Division;
                        rank += $"{value.Tier.ToString().ToLower()} {division.LeagueDivisionInt()} ";
                    }
                    var game = riotClient.CurrentGame.GetCurrentGameInformationForSummonerId((RiotApiConfig.Platforms)int.Parse(e.Args[1]), player.Id);
                    var thePlayer = game.Participants.First(x => x.SummonerName == player.Name);
                    var participants = game.Participants.Where(x => x.TeamId != thePlayer.TeamId);
                    foreach (var participant in participants)
                    {
                        foreach (var rune in participant.Runes)
                            runes += $"[{AllRunes.Data.First(x => x.Value.Id == rune.RuneId).Value.Name}({rune.Count})]\n";

                        listParticipants += $"{participant.SummonerName.ToLower()} Playing: {riotClient.LolStaticData.GetChampionById(RiotApiConfig.Regions.NA, (int)participant.ChampionId).Name.ToLower().Replace('\'', ' ')}\n{runes}\n";
                        runes = "";
                    }
                    await e.Channel.SendMessage($"```xl\nName: {player.Name.ToLower()} ID: {player.Id} Level: {player.SummonerLevel} Ranks: {rank} Current game: {game.GameMode} - {game.GameLength / 60 + 3} Min\nPlayers\n{listParticipants}```");

                }
                catch (Exception)
                {
                    await e.Channel.SendMessage($"```xl\nName: {player.Name.ToLower()} ID: {player.Id} Level: {player.SummonerLevel} Rank: {rank}```");
                    await e.Channel.SendMessage($"```No game found for {player.Name}```");
                }
            });
            _client.GetService<CommandService>().CreateCommand("topplayers")
            .Alias(new string[] { "top", "top5" })
            .Description("Get's the top players from a division.")
            .Parameter("region", ParameterType.Required)
            .Parameter("division", ParameterType.Required)

            .Do(async e =>
            {
                IRiotClient riotClient = new RiotClient(lolApi);

                try
                {
                    if (e.Args[1].ToLower() == "challenger")
                    {
                        var challengers = riotClient.League.GetChallengerTierLeagues((RiotApiConfig.Regions)int.Parse(e.Args[0]), RiotApi.Net.RestClient.Helpers.Enums.GameQueueType.RANKED_SOLO_5x5);
                        string message = "[Top 5 Challenger]\n";
                        for (int i = 1; i <= 5; i++)
                        {
                            message += $"({i}) Name: {challengers.Entries.ToList()[i].PlayerOrTeamName.ToLower()} {challengers.Entries.ToList()[i].Wins} Wins {challengers.Entries.ToList()[i].Losses} Losses {challengers.Entries.ToList()[i].LeaguePoints} Points\n";
                        }
                        await e.Channel.SendMessage($"```xl\n{message}```");
                    }
                    else if (e.Args[1].ToLower() == "master")
                    {
                        var masters = riotClient.League.GetMasterTierLeagues((RiotApiConfig.Regions)int.Parse(e.Args[0]), RiotApi.Net.RestClient.Helpers.Enums.GameQueueType.RANKED_SOLO_5x5);
                        string message = "[Top 5 Master]\n";
                        for (int i = 1; i <= 5; i++)
                        {
                            message += $"({i}) Name: {masters.Entries.ToList()[i].PlayerOrTeamName.ToLower()} {masters.Entries.ToList()[i].Wins} Wins {masters.Entries.ToList()[i].Losses} Losses {masters.Entries.ToList()[i].LeaguePoints} Points\n";
                        }
                        await e.Channel.SendMessage($"```xl\n{message}```");
                    }
                    else
                    {
                        throw new Exception("You must choose between master and challenger.");
                    }
                }
                catch (Exception ex)
                {
                    await e.Channel.SendMessage($"```{ex.Message}```");
                }
            });
            #endregion
        }

        static public async Task
        ReadAudio(string text, Message e)
        {
            var voiceChannel = e.Server.VoiceChannels.FirstOrDefault();
            if (e.User.VoiceChannel != null && e.User.VoiceChannel.Name.ToLower() != "afk")
                voiceChannel = e.User.VoiceChannel;
            var _vClient = await voiceChannel.JoinAudio();
            var channelCount = 2;
            var OutFormat = new WaveFormat(48000, 16, channelCount);
            SpeechSynthesizer synth = new SpeechSynthesizer();
            MemoryStream ms = new MemoryStream();
            synth.SetOutputToAudioStream(ms, new SpeechAudioFormatInfo(48000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
            synth.Speak(text);
            byte[] bytes = ms.ToArray();
            var test = new RawSourceWaveStream(ms, OutFormat);
            test.Seek(0, SeekOrigin.Begin);
            using (var resampler = new MediaFoundationResampler(test, OutFormat))
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

        static public async Task
        SendAudio(string filePath, Message e)
        {
            try
            {
                var voiceChannel = e.Server.VoiceChannels.FirstOrDefault();
                if (e.User.VoiceChannel != null && e.User.VoiceChannel.Name.ToLower() != "afk")
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

