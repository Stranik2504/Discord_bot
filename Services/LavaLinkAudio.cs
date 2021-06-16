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

        public async Task<object> JoinAsync(IGuild guild, IVoiceChannel voiceChannel, ITextChannel textChannel)
        {
            var nameCommand = "join command";

            if (voiceChannel is null)
                return (await EmbedHandler.CreateErrorEmbed(nameCommand, "You must be connected to a voice channel!"));

            if (_lavaNode.HasPlayer(guild) && voiceChannel.Id == _lavaNode.GetPlayer(guild).VoiceChannel.Id)
                return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm already connected to a voice channel!");

            try
            {
                await _lavaNode.JoinAsync(voiceChannel, textChannel);
                await _lavaNode.GetPlayer(guild).UpdateVolumeAsync(GetVolume(guild));
                return $"Join in {voiceChannel.Name}.";
            }
            catch (Exception ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task<object> PlayAsync(IGuild guild, IVoiceChannel voiceChannel, string query)
        {
            var nameCommand = "play command";

            if (voiceChannel == null)
                return await EmbedHandler.CreateErrorEmbed(nameCommand, "You must first join a voice channel.");

            if (!_lavaNode.HasPlayer(guild))
                if (voiceChannel != null)
                    await _lavaNode.JoinAsync(voiceChannel);
                else
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

            try
            {
                IReadOnlyList<LavaTrack> tracks;
                var player = _lavaNode.GetPlayer(guild);

                await player.UpdateVolumeAsync(GetVolume(guild));

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ? await _lavaNode.SearchAsync(query) : await _lavaNode.SearchYouTubeAsync(query);

                if (search.LoadStatus == LoadStatus.NoMatches)
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, $"I wasn't able to find anything for {query}.");

                tracks = search.Tracks;

                if (tracks == null || tracks.Count == 0) 
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, $"I wasn't able to find anything for {query}.");

                tracks.ToList().ForEach(x => player.Queue.Enqueue(x));

                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    if (search.Playlist.Name == null)
                    {
                        await LoggingService.LogInformationAsync(nameCommand, $"{tracks.FirstOrDefault().Title} has been added to the music queue.");
                        return await EmbedHandler.CreateMusicEmbed(tracks.FirstOrDefault().Title, tracks.FirstOrDefault().Url, tracks.FirstOrDefault().Duration.ToString(), player.Queue.Count.ToString(), 1, Color.Green);
                    }
                    else
                    {
                        await LoggingService.LogInformationAsync(nameCommand, $"Playlist {search.Playlist.Name} has been added to the music queue.");
                        return await EmbedHandler.CreateMusicEmbed(search.Playlist.Name, tracks.FirstOrDefault().Url, default, player.Queue.Count.ToString(), tracks.Count, Color.Green);
                    }
                }

                player.Queue.TryDequeue(out LavaTrack track);
                await player.PlayAsync(track);

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
                if (!_lavaNode.HasPlayer(guild))
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                var player = _lavaNode.GetPlayer(guild);

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

                
                if (!_lavaNode.HasPlayer(guild))
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                var player = _lavaNode.GetPlayer(guild);

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
                if (!_lavaNode.HasPlayer(guild))
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                var player = _lavaNode.GetPlayer(guild);

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
                if (!_lavaNode.HasPlayer(guild))
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                var player = _lavaNode.GetPlayer(guild);

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

            GlobalData.Config.SetNewVoulme(guild.Id, (ushort)volume);
            GlobalData.Save();

            try
            {
                if (!_lavaNode.HasPlayer(guild))
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                var player = _lavaNode.GetPlayer(guild);

                await player.UpdateVolumeAsync(GetVolume(guild));

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
                if (!_lavaNode.HasPlayer(guild))
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                var player = _lavaNode.GetPlayer(guild);

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
                if (!_lavaNode.HasPlayer(guild))
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                var player = _lavaNode.GetPlayer(guild);

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

            if (queueable is not LavaTrack track)
            {
                await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track");
                return;
            }

            await args.Player.PlayAsync(track); 
            await args.Player.TextChannel.SendMessageAsync(embed: await EmbedHandler.CreatePlayingEmbed(track.Title, track.Url, track.Duration.ToString(), track.Author, Color.Blue));
        }

        private ushort GetVolume(IGuild guild) => GlobalData.Config.GetVoulme(guild.Id);
    }
}
