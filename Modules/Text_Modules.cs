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
        public CommandService Commands { get; set; }

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
        [Alias("help", "command", "commands")]
        [Summary("Команда для получения информации о командах")]
        public async Task HelpCommand()
        {
            string output = "```";
            //CommandService Commands = default;

            for (int i = 0; i < Commands.Commands.Count(); i++)
            {
                var item = Commands.Commands.ToList()[i];

                output += $"{i + 1}) {item.Name}";

                if (item.Aliases.Count > 0)
                {
                    output += "(";
                    for (int j = 1; j < item.Aliases.Count; j++) { output += item.Aliases[j]; if (j + 1 < item.Aliases.Count) output += ", "; }
                    output += ")";
                }

                if (item.Summary != null)
                {
                    output += $":\n{item.Summary}";
                }
                else output += "\n";
            }

            output += "```";

            await ReplyAsync(output);
        }
    }
}
