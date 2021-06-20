
using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            HookEvents();
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services);

            //_commands.Commands.ToList().ForEach(x => System.Diagnostics.Debug.WriteLine(x.Module.Name));
        }

        public void HookEvents()
        {
            _commands.CommandExecuted += CommandExecutedAsync;
            _commands.Log += LogAsync;

            _client.MessageReceived += HandleCommandAsync;
        }

        private Task HandleCommandAsync(SocketMessage socketMessage)
        {
            var argPos = 0;
            if (socketMessage is not SocketUserMessage message || message.Author.IsBot || message.Author.IsWebhook) return Task.CompletedTask;

            var context = new SocketCommandContext(_client, socketMessage as SocketUserMessage);

            if (!message.HasStringPrefix(GlobalData.Config.GetPrefix(context.Guild.Id), ref argPos)) return Task.CompletedTask;

            var result = _commands.ExecuteAsync(context, argPos, _services, MultiMatchHandling.Best);

            if (!result.Result.IsSuccess)
            {
                context.Channel.SendMessageAsync(result.Result.ErrorReason);
            }

            return result;
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
