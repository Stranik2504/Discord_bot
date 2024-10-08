﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Victoria;

namespace Discord_Bot.Services
{
    public class BotService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _commandHandler;
        private readonly ServiceProvider _services;
        private readonly LavaNode _lavaNode;
        private readonly LavaLinkAudio _audioService;

        private readonly Discord.Interactions.InteractionService _interactionService;

        public BotService()
        {
            _services = ConfigureServices();

            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commandHandler = _services.GetRequiredService<CommandHandler>();
            _lavaNode = _services.GetRequiredService<LavaNode>();
            _audioService = _services.GetRequiredService<LavaLinkAudio>();
            _interactionService = _services.GetRequiredService<Discord.Interactions.InteractionService>();

            _lavaNode.OnLog += LogAsync;
            _lavaNode.OnTrackEnded += _audioService.TrackEnded;

            _client.Log += LogAsync;
            _client.Ready += async () => {
                await ReadyAsync();
                await _interactionService.RegisterCommandsGloballyAsync();
                await _interactionService.RegisterCommandsToGuildAsync(775270040907284483);
                await _interactionService.RegisterCommandsToGuildAsync(751458906030604339);

                await Task.Run(() => {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[Bot started]");
                    Console.ResetColor();
                });
            };
        }

        public async Task Init()
        {
            await GlobalData.LoadAsync();

            await _client.LoginAsync(TokenType.Bot, GlobalData.Config.Token);
            await _client.StartAsync();

            await _commandHandler.InitializeAsync();

            await Task.Delay(-1);
        }

        private async Task ReadyAsync()
        {
            try
            {
                await _lavaNode.ConnectAsync();
                if (GlobalData.Config.GameStatus != default) await _client.SetGameAsync(GlobalData.Config.GameStatus);
            }
            catch (Exception ex) { await LoggingService.LogInformationAsync(ex.Source, ex.Message); }
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddLavaNode(x => { x.SelfDeaf = false; })
                .AddSingleton(new LavaConfig() { LogSeverity = LogSeverity.Info })
                .AddSingleton<LavaLinkAudio>()
                .AddSingleton<BotService>()
                .AddSingleton<GlobalData>()
                .AddSingleton<Discord.Interactions.InteractionService>()
                .BuildServiceProvider();
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message, logMessage.Exception);
        }
    }
}
