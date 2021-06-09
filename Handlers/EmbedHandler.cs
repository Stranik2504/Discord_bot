using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Handlers
{
    public class EmbedHandler
    {
        public static async Task<Embed> CreateBasicEmbed(string title, string description, Color color) => 
            await Task.Run(() => new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithCurrentTimestamp().Build());

        public static async Task<Embed> CreateMusicEmbed(string title, string url, string length, string position, int count, Color color) =>
            await Task.Run(() => new EmbedBuilder()
                .WithTitle($"{(count == 1 ? "Song" : "Playlist")} added to queue")
                .AddField(title, url)
                .AddField("Length: ", null, true)
                .AddField(length, null)
                .AddField("Position: ", null, true)
                .AddField(position, null)
                .AddField("Enqueued: ", null, true)
                .AddField(count.ToString(), null)
                .WithColor(color)
                .WithCurrentTimestamp().Build());

        public static async Task<Embed> CreatePlayingEmbed(string title, string url, string length, string author, Color color) =>
            await Task.Run(() => new EmbedBuilder()
                .WithTitle("Now Playing :musical_note:")
                .AddField(title, url)
                .AddField($"Length: {length}", null)
                .AddField($"Author: {author}", null)
                .WithColor(color)
                .WithCurrentTimestamp().Build());

        public static async Task<Embed> CreateErrorEmbed(string source, string error)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle($"Error in {source}")
                .WithDescription($"**Error Deaitls**: \n{error}")
                .WithColor(Color.DarkRed)
                .WithCurrentTimestamp().Build());
            return embed;
        }
    }
}
