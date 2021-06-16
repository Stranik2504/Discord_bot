using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly GlobalData _globalData;

        public BotService()
        {
            _services = ConfigureServices();

            _client = new DiscordSocketClient();
            _commandHandler = _services.GetRequiredService<CommandHandler>();
            _lavaNode = _services.GetRequiredService<LavaNode>();
            _globalData = _services.GetRequiredService<GlobalData>();
            _audioService = _services.GetRequiredService<LavaLinkAudio>();

            //_lavaNode.OnLog += LogAsync;
            //_lavaNode.OnTrackEnded += _audioService.TrackEnded;

            _client.Log += LogAsync;
            _client.Ready += async () => {
                await ReadyAsync();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[Bot started]");
                Console.ResetColor();
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
                //await _lavaNode.ConnectAsync();
                if (GlobalData.Config.GameStatus != default) await _client.SetGameAsync(GlobalData.Config.GameStatus);
            }
            catch (Exception ex) { await LoggingService.LogInformationAsync(ex.Source, ex.Message); }
        }

        private ServiceProvider ConfigureServices() => new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<LavaNode>()
                .AddSingleton(new LavaConfig())
                .AddSingleton<LavaLinkAudio>()
                .AddSingleton<BotService>()
                .AddSingleton<GlobalData>()
                .BuildServiceProvider();

        private async Task LogAsync(LogMessage logMessage)
        {
            await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message, logMessage.Exception);
        }
    }

}
