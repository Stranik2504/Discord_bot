
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Modules;
using Discord_Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord_Bot.Handlers
{
    public class CommandHandler
    {
        public static bool Unmute = false;
        public static bool Mute = false;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        private readonly Discord.Interactions.InteractionService _interactionService;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _interactionService = services.GetRequiredService<Discord.Interactions.InteractionService>();
            _services = services;

            HookEvents();
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services);

            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public void HookEvents()
        {
            _commands.CommandExecuted += CommandExecutedAsync;
            _client.SlashCommandExecuted += async (command) =>
            {
                var ctx = new Discord.Interactions.SocketInteractionContext<SocketSlashCommand>(_client, command);
                await _interactionService.ExecuteCommandAsync(ctx, _services);
            };

            _commands.Log += LogAsync;

            _client.MessageReceived += HandleCommandAsync;
            _client.UserVoiceStateUpdated += async (socketUser, oldStatus, newStatus) =>
            {
                var user = socketUser as SocketGuildUser;

                /*if (user.VoiceState.HasValue && (user.VoiceState.Value.IsMuted == false || user.VoiceState.Value.IsDeafened == false) && user.Id == 307764896219791360)
                    await user.ModifyAsync(x => { x.Mute = true; x.Deaf = true; });*/

                //if (user.VoiceState.HasValue && user.Id == 307764896219791360)
                //await user.

                if (Unmute)
                {
                    if (user.VoiceState.HasValue && user.VoiceState.Value.IsMuted == true || user.VoiceState.Value.IsDeafened == true && user.Id == 452597784516886538)
                        await user.ModifyAsync(x => { x.Mute = false; x.Deaf = false; });
                }
                
            };
            
        }

        private async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            var argPos = 0;
            if (socketMessage is not SocketUserMessage message || message.Author.IsBot || message.Author.IsWebhook) 
                return;

            var context = new SocketCommandContext(_client, socketMessage as SocketUserMessage);

            if (context.Message.Content.ToLower() == "привет")
            {
                await context.Message.ReplyAsync("Привет");
                return;
            }

            if (!message.HasStringPrefix(GlobalData.Config.GetPrefix(context.Guild.Id), ref argPos)) 
                return;

            var result = _commands.ExecuteAsync(context, argPos, _services, MultiMatchHandling.Best);

            return;
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified || result.IsSuccess) return;

            await context.Channel.SendMessageAsync(embed: await EmbedHandler.CreateErrorEmbed("execute command", result.ErrorReason));
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message, logMessage.Exception);
        }
    }
}
