using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Handlers;
using Discord_Bot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Search;

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

        public async Task<object> PlayAsync(SocketCommandContext context, IVoiceChannel voiceChannel, string query)
        {
            var nameCommand = "play command";
            var guild = context.Guild;

            if (voiceChannel == null)
                return await EmbedHandler.CreateErrorEmbed(nameCommand, "You must first join a voice channel.");

            if (!_lavaNode.HasPlayer(guild))
                if (voiceChannel != null)
                    await _lavaNode.JoinAsync(voiceChannel, context.Channel as ITextChannel);
                else
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

            try
            {
                IReadOnlyCollection<LavaTrack> tracks;
                var player = _lavaNode.GetPlayer(guild);

                await player.UpdateVolumeAsync(GetVolume(guild));

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ? await _lavaNode.SearchAsync(SearchType.Direct, query) : await _lavaNode.SearchYouTubeAsync(query);

                if (search.Status == SearchStatus.NoMatches)
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, $"I wasn't able to find anything for {query}.");

                tracks = search.Tracks;

                if (tracks == null || tracks.Count == 0) 
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, $"I wasn't able to find anything for {query}.");

                if (search.Playlist.Name != null)
                    tracks.ToList().ForEach(x => player.Queue.Enqueue(x));
                else
                {
                    player.Queue.Enqueue(tracks.First());
                    //TODO: добавить выбор песен
                }

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
                        return await EmbedHandler.CreateMusicEmbed(search.Playlist.Name, default, default, player.Queue.Count.ToString(), tracks.Count, Color.Green);
                    }
                }

                player.Queue.TryDequeue(out LavaTrack track);
                await player.PlayAsync(track);

                if (search.Playlist.Name != null)
                {
                    await LoggingService.LogInformationAsync(nameCommand, $"Playlist {search.Playlist.Name} has been added to the music queue.");
                    await Music_Modules.Print(context, await EmbedHandler.CreateMusicEmbed(search.Playlist.Name, default, default, (player.Queue.Count - tracks.Count + 1).ToString(), tracks.Count, Color.Green));
                }

                await LoggingService.LogInformationAsync(nameCommand, $"Bot Now Playing: {track.Title}; Url: {track.Url}");
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
                var descriptionBuilderOutput = new StringBuilder();

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

                        if (trackNum == 10 || descriptionBuilder.Capacity >= 2048) break;

                        descriptionBuilderOutput.Append($"{trackNum - 1}: [{track.Title}]({track.Url})\n");
                    }

                    return await EmbedHandler.CreateBasicEmbed("Queue", $"List to play: \n1: [{player.Track.Title}]({player.Track.Url}) \n{descriptionBuilderOutput}", Color.Blue);
                }

                return "Player doesn't seem to be playing anything right now";
            }
            catch (Exception ex) { return await EmbedHandler.CreateErrorEmbed(nameCommand, ex.Message); }
        }

        public async Task<object> SkipTrackAsync(IGuild guild, int count)
        {
            var nameCommand = "skip command";

            try
            {
                if (!_lavaNode.HasPlayer(guild))
                    return await EmbedHandler.CreateErrorEmbed(nameCommand, "I'm not connected to a voice channel.");

                var player = _lavaNode.GetPlayer(guild);

                if (player.Queue.Count == 0 && player.PlayerState != PlayerState.Playing) return "Queue is clear";

                try
                {
                    if (count == 1)
                    {
                        var currentTrack = player.Track;
                        await player.SkipAsync();

                        await LoggingService.LogInformationAsync(nameCommand, $"Bot skipped: {currentTrack.Title}");
                        return $"I have skiped {currentTrack.Title}";
                    }
                    else
                    {
                        for (int i = 0; i < count; i++) { if (player.Queue.Count > 0) await player.SkipAsync(); else break; }

                        await LoggingService.LogInformationAsync(nameCommand, $"Bot skipped: {count} tracks");
                        return $"I have skiped {count} tracks";
                    }
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
            if (args.Reason != TrackEndReason.Finished) return;

            if (!args.Player.Queue.TryDequeue(out var queueable))
            {
                if (GlobalData.Config.GetNeedOutput(args.Player.VoiceChannel.GuildId))
                    await args.Player.TextChannel.SendMessageAsync("Playing finished");
                return;
            }

            if (queueable is not LavaTrack track)
            {
                if (GlobalData.Config.GetNeedOutput(args.Player.VoiceChannel.GuildId))
                    await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track");
                return;
            }

            await args.Player.PlayAsync(track);

            if (args.Player != null && args.Player.TextChannel != null && GlobalData.Config.GetNeedOutput(args.Player.VoiceChannel.GuildId))
                await args.Player.TextChannel.SendMessageAsync(embed: await EmbedHandler.CreatePlayingEmbed(track.Title, track.Url, track.Duration.ToString(), track.Author, Color.Blue));
        }

        private ushort GetVolume(IGuild guild) => GlobalData.Config.GetVoulme(guild.Id);
    }
}
