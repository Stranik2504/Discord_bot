using Discord;
using Discord.Commands;
using Discord_Bot.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Modules
{
    public class Text_Modules : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        //[Alias("pong")]
        //[Summary("ghj")]
        public async Task Pong()
        {
            await ReplyAsync("PONG!");
        }

        [Command("prefix")]
        public async Task ChangePrefix(string prefix)
        {
            GlobalData.Config.SetNewPrefix(Context.Guild.Id, prefix);
            GlobalData.Save();

            await ReplyAsync("Prefix changed to " + prefix);
        }

        [Command("outputs")]
        public async Task ChangeOutputNameSong(bool output)
        {
            GlobalData.Config.SetNewNeedOutput(Context.Guild.Id, output);
            GlobalData.Save();

            await ReplyAsync("Output name next song to " + output);
        }

        [Command("commands")]
        public async Task Commands()
        {
            await ReplyAsync("LOL");
        }
    }
}
