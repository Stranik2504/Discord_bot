using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Modules
{
    public class Music_Modules : ModuleBase<SocketCommandContext>
    {
        public LavaLinkAudio AudioService { get; set; }

        [Command("Command to join")]
        [Alias("join")]
        public async Task JoinAndPlay() => await Output(await AudioService.JoinAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel, Context.Channel as ITextChannel));

        [Command("Leave")]
        public async Task Leave() => await Output(await AudioService.LeaveAsync(Context.Guild));

        [Command("Play")]
        public async Task Play([Remainder] string search) => await Output(await AudioService.PlayAsync(Context, (Context.User as SocketGuildUser).VoiceChannel, search));

        [Command("Stop")]
        public async Task Stop() => await Output(await AudioService.StopAsync(Context.Guild));

        [Command("List")]
        public async Task List() => await Output(await AudioService.ListAsync(Context.Guild));

        [Command("Skip")]
        public async Task Skip([Remainder] int count = 1) => await Output(await AudioService.SkipTrackAsync(Context.Guild, count));

        [Command("Volume")]
        public async Task Volume(int volume) => await Output(await AudioService.SetVolumeAsync(Context.Guild, volume));

        [Command("Pause")]
        public async Task Pause() => await Output(await AudioService.PauseAsync(Context.Guild));

        [Command("Resume")]
        public async Task Resume() => await Output(await AudioService.ResumeAsync(Context.Guild));

        public static async Task Print(SocketCommandContext context, Embed embed) => await context.Channel.SendMessageAsync(embed: embed);

        private async Task Output(object output)
        {
            if (output is string)
                await ReplyAsync(message: output as string);
            else if (output is Embed)
                await ReplyAsync(embed: output as Embed);
        }
    }
}
