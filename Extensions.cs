using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace BotRoss
{
    public static class Extensions
    {
        public static Task Reply(this DiscordClient client, CommandEventArgs e, string text)
            => Reply(client, e.User, e.Channel, text);
        public async static Task Reply(this DiscordClient client, User user, Channel channel, string text)
        {
            if (text != null)
            {
                if (!channel.IsPrivate)
                    await channel.SendMessage($"{user.Name}: {text}");
                else
                    await channel.SendMessage(text);
            }
        }
        public static Task Reply<T>(this DiscordClient client, CommandEventArgs e, string prefix, T obj)
            => Reply(client, e.User, e.Channel, prefix, obj != null ? JsonConvert.SerializeObject(obj, Formatting.Indented) : "null");
        public static Task Reply<T>(this DiscordClient client, User user, Channel channel, string prefix, T obj)
            => Reply(client, user, channel, prefix, obj != null ? JsonConvert.SerializeObject(obj, Formatting.Indented) : "null");
        public static Task Reply(this DiscordClient client, CommandEventArgs e, string prefix, string text)
            => Reply(client, e.User, e.Channel, (prefix != null ? $"{Format.Bold(prefix)}:\n" : "\n") + text);
        public static Task Reply(this DiscordClient client, User user, Channel channel, string prefix, string text)
            => Reply(client, user, channel, (prefix != null ? $"{Format.Bold(prefix)}:\n" : "\n") + text);

        public static Task ReplyError(this DiscordClient client, CommandEventArgs e, string text)
            => Reply(client, e.User, e.Channel, "Error: " + text);
        public static Task ReplyError(this DiscordClient client, User user, Channel channel, string text)
            => Reply(client, user, channel, "Error: " + text);
        public static Task ReplyError(this DiscordClient client, CommandEventArgs e, Exception ex)
            => Reply(client, e.User, e.Channel, "Error: " + ex.GetBaseException().Message);
        public static Task ReplyError(this DiscordClient client, User user, Channel channel, Exception ex)
            => Reply(client, user, channel, "Error: " + ex.GetBaseException().Message);
    }

    internal static class InternalExtensions
    {
        public static Task<User[]> FindUsers(this DiscordClient client, CommandEventArgs e, string username, string discriminator)
            => FindUsers(client, e, username, discriminator, false);
        public static async Task<User> FindUser(this DiscordClient client, CommandEventArgs e, string username, string discriminator)
            => (await FindUsers(client, e, username, discriminator, true))?[0];
        public static async Task<User[]> FindUsers(this DiscordClient client, CommandEventArgs e, string username, string discriminator, bool singleTarget)
        {
            IEnumerable<User> users;
            if (discriminator == "")
                users = e.Server.FindUsers(username);
            else
            {
                var user = e.Server.GetUser(username, ushort.Parse(discriminator));
                if (user == null)
                    users = Enumerable.Empty<User>();
                else
                    users = new User[] { user };
            }

            int count = users.Count();
            if (singleTarget)
            {
                if (count == 0)
                {
                    await client.ReplyError(e, "User was not found.");
                    return null;
                }
                else if (count > 1)
                {
                    await client.ReplyError(e, "Multiple users were found with that username.");
                    return null;
                }
            }
            else
            {
                if (count == 0)
                {
                    await client.ReplyError(e, "No user was found.");
                    return null;
                }
            }
            return users.ToArray();
        }
        public static async Task<Channel> FindChannel(this DiscordClient client, CommandEventArgs e, string name, ChannelType type = null)
        {
            var channels = e.Server.FindChannels(name, type);

            int count = channels.Count();
            if (count == 0)
            {
                await client.ReplyError(e, "Channel was not found.");
                return null;
            }
            else if (count > 1)
            {
                await client.ReplyError(e, "Multiple channels were found with that name.");
                return null;
            }
            return channels.FirstOrDefault();
        }

        public static async Task<User> GetUser(this DiscordClient client, CommandEventArgs e, ulong userId)
        {
            var user = e.Server.GetUser(userId);

            if (user == null)
            {
                await client.ReplyError(e, "No user was not found.");
                return null;
            }
            return user;
        }

        public static string ToReadableAgeString(this TimeSpan span)
        {
            return string.Format("{0:0}", span.Days / 365.25);
        }

        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        public static string ToReadableMemory(this long memory)
        {
            string formatted = (memory / 1048576).ToString() + "Mb";
            return formatted;
        }
    }
}
