using Discord;
using Discord.Commands;
using Discord_Bot.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Z.Expressions;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using Discord_Bot.Attributes;

using static System.Diagnostics.Debug;

namespace Discord_Bot.Modules
{
    public class Text_Modules : ModuleBase<SocketCommandContext>
    {
        public CommandService Commands { get; set; }

        [Command("ping")]
        public async Task Pong()
        {
            await ReplyAsync("PONG!");
        }

        [Command("prefix")]
        [Accesses(452597784516886538, Access.Admin)]
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

        [Command("eval")]
        [Accesses(Access.Admin)]
        public async Task Evals([Remainder] string text)
        {
            await ReplyAsync(ExpressionEvaluator.Eval(text));
        }

        [Command("send")]
        [Accesses(Access.Admin)]
        public async Task Send([Remainder] string text)
        {
            await Context.Message.DeleteAsync();
            Console.WriteLine(text);
            await ReplyAsync(text);
        }

        [Accesses(452597784516886538)]
        public async Task AddAccess([Remainder] ulong userId)
        {
            await ReplyAsync("you do it");
        }


        [Command("relaod")]
        [Accesses(452597784516886538)]
        public async Task ReloadConf()
        {
            await GlobalData.LoadAsync();
        }

        [Command("help")]
        [Alias("command", "commands", "commands")]
        [Summary("Команда для получения информации о командах")]
        public async Task HelpCommand()
        {
            string output = "```";
            //CommandService Commands = default;
            List<CommandInfo> commands = new();
            Commands.Commands.ToList().ForEach(x => {
                var isAdd = true;

                x.Preconditions.ToList().ForEach(y =>
                {
                    var res = y.CheckPermissionsAsync(Context, x, null).Result;

                    if (!res.IsSuccess)
                        isAdd = false;
                });

                if (isAdd)
                    commands.Add(x);
            });


            for (int i = 0; i < commands.Count(); i++)
            {
                var item = commands.ToList()[i];

                output += $"{i + 1}) {item.Name}";

                if (item.Aliases.Count > 0)
                {
                    output += "(";
                    for (int j = 1; j < item.Aliases.Count; j++) { output += item.Aliases[j]; if (j + 1 < item.Aliases.Count) output += ", "; }
                    output += ")";
                }

                if (item.Summary != null)
                {
                    output += $":\n\t{item.Summary}";
                }
                else output += "\n";
            }

            output += "```";

            await ReplyAsync(output);
        }
    }

    public class ExpressionEvaluator
    {
        public static string Eval(string code)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerResults results =
                codeProvider
                .CompileAssemblyFromSource(new CompilerParameters(), new string[] { code });

            Assembly assembly = results.CompiledAssembly;
            dynamic evaluator =
                Activator.CreateInstance(assembly.GetType("MyAssembly.Evaluator"));
            return evaluator.Eval();
        }
    }
}
