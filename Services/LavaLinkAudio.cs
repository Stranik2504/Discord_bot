using Discord;
using Discord.WebSocket;
using Discord_Bot.Handlers;
using Discord_Bot.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace Discord_Bot.Services
{
    public sealed class LavaLinkAudio
    {
        private readonly LavaNode _lavaNode;

        public LavaLinkAudio(LavaNode lavaNode) => _lavaNode = lavaNode;

        public async Task<object> JoinAsync(IGuild guild, IVoiceChannel voiceChannel)
        {
            var nameCommand = "join command";

            if (voiceChannel is null)
                return (await EmbedHandler.CreateErrorEmbed(nameCommand, "You must be connected to a voice channel!"));

            var player = _lavaNode.GetPlayer(guild);
            if (player != null && voiceChannel.Id == player.VoiceChannel.Id)
                return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm already connected to a voice channel!");

            try
            {
                await _lavaNode.JoinAsync(voiceChannel);
                return $"Join in {voiceChannel.Name}.";
            }
            catch (Exception ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task<object> PlayAsync(IGuild guild, IVoiceChannel voiceChannel, string query)
        {
            var nameCommand = "play command";

            if (voiceChannel == null)
                return await EmbedHandler.CreateErrorEmbed(nameCommand, "You must first join a voice channel.");

            var player = _lavaNode.GetPlayer(guild);
            if (player != null)
                if (voiceChannel != null)
                    await _lavaNode.JoinAsync(voiceChannel);
                else
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

            try
            {
                LavaTrack track;

                var search = await _lavaNode.SearchAsync(query);

                if (search.LoadStatus == LoadStatus.NoMatches)
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, $"I wasn't able to find anything for {query}.");

                track = search.Tracks?.FirstOrDefault();

                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    await LoggingService.LogInformationAsync(nameCommand, $"{track.Title} has been added to the music queue.");
                    return await EmbedHandler.CreateMusicEmbed(track.Title, track.Url, track.Duration.ToString(), player.Queue.Count.ToString(), 1, Color.Blue);
                }

                await player.PlayAsync(track);
                await LoggingService.LogInformationAsync(nameCommand, $"Bot Now Playing: {track.Title}\nUrl: {track.Url}");
                return await EmbedHandler.CreatePlayingEmbed(track.Title, track.Url, track.Duration.ToString(), track.Author, Color.Blue);
            }
            catch (Exception ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task<object> LeaveAsync(IGuild guild)
        {
            var nameCommand = "leave command";

            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player != null)
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                if (player.PlayerState is PlayerState.Playing) await player.StopAsync();

                await _lavaNode.LeaveAsync(player.VoiceChannel);

                await LoggingService.LogInformationAsync(nameCommand, $"Bot has left.");
                return $"I've left";
            }
            catch (InvalidOperationException ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task<object> ListAsync(IGuild guild)
        {
            var nameCommand = "queue command";

            try
            {
                var descriptionBuilder = new StringBuilder();

                var player = _lavaNode.GetPlayer(guild);
                if (player != null)
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                if (player == null) return $"Could not acquire queue";

                if (player.PlayerState is PlayerState.Playing)
                {
                    if (player.Queue.Count < 1 && player.Track != null) return $"List to play: {player.Track.Title}";

                    var trackNum = 2;
                    foreach (LavaTrack track in player.Queue)
                    {
                        descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Url})\n");
                        trackNum++;
                    }

                    return await EmbedHandler.CreateBasicEmbed(nameCommand, $"List to play: [{player.Track.Title}]({player.Track.Url}) \n{descriptionBuilder}", Color.Blue);
                }

                return "Player doesn't seem to be playing anything right now";
            }
            catch (Exception ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task<object> SkipTrackAsync(IGuild guild)
        {
            var nameCommand = "skip command";

            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player != null)
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                if (player.Queue.Count < 1) return "Queue is clear";

                try
                {
                    var currentTrack = player.Track;
                    await player.SkipAsync();

                    await LoggingService.LogInformationAsync(nameCommand, $"Bot skipped: {currentTrack.Title}");
                    return "I have skiped {currentTrack.Title}";
                }
                catch (Exception ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
            }
            catch (Exception ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task<object> StopAsync(IGuild guild)
        {
            var nameCommand = "stop command";

            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player != null)
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                if (player.PlayerState is PlayerState.Playing) await player.StopAsync();

                await LoggingService.LogInformationAsync(nameCommand, $"Bot has stopped playback.");
                return "I Have stopped playback and the playlist has been cleared";
            }
            catch (Exception ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task<object> SetVolumeAsync(IGuild guild, int volume)
        {
            var nameCommand = "set volume command";

            if (volume > 150 || volume <= 0) return $"Volume must be between 1 and 150.";

            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player != null)
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                await player.UpdateVolumeAsync((ushort)volume);

                await LoggingService.LogInformationAsync(nameCommand, $"Bot Volume set to: {volume}");
                return $"Volume has been set to {volume}";
            }
            catch (InvalidOperationException ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task<object> PauseAsync(IGuild guild)
        {
            var nameCommand = "pause command";

            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player != null)
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                if (!(player.PlayerState is PlayerState.Playing))
                {
                    await player.PauseAsync();
                    return $"There is nothing to pause.";
                }

                await player.PauseAsync();
                return $"**Paused:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task<object> ResumeAsync(IGuild guild)
        {
            var nameCommand = "resume command";

            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player != null)
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                if (player.PlayerState is PlayerState.Paused) await player.ResumeAsync();

                return $"**Resumed:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task TrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext()) return;

            if (!args.Player.Queue.TryDequeue(out var queueable))
            {
                await args.Player.TextChannel.SendMessageAsync("Playing finished");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track");
                return;
            }

            await args.Player.PlayAsync(track); 
            await args.Player.TextChannel.SendMessageAsync(embed: await EmbedHandler.CreatePlayingEmbed(track.Title, track.Url, track.Duration.ToString(), track.Author, Color.Blue));
        }
    }
}
