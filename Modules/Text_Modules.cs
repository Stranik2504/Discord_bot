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
using Discord.WebSocket;

namespace Discord_Bot.Modules
{
    public class SlashTextModule : Discord.Interactions.InteractionModuleBase<Discord.Interactions.SocketInteractionContext<SocketSlashCommand>>
    {
        /*[Discord.Interactions.ComponentInteraction("send")]
        [Discord.Interactions.SlashCommand("send", "Команда для отправки сообщения от имени бота")]
        [Accesses(Access.Admin)]
        [Summary("Команда для отправки сообщения от имени бота")]
        public async Task Send([Remainder] string text)
        {
            await Context.Interaction.RespondAsync(text);
        }*/

        [Discord.Interactions.SlashCommand("cnt_last", "Команда для получения количеста сообщений одного типа из последних 100")]
        [AccessesUser(452597784516886538)]
        public async Task CountLast()
        {
            var messages = (await Context.Channel.GetMessagesAsync(100).FlattenAsync()).ToList();

            var typeMessToDelete = messages.ToList()[0];
            var cnt = 0;

            for (int i = 0; i < messages.Count; i++)
            {
                if (typeMessToDelete.Content == messages[i].Content)
                    cnt++;
            }

            await Context.Interaction.RespondAsync($"Count: {cnt}");
        }
    }

    public class Text_Modules : ModuleBase<SocketCommandContext>
    {
        public CommandService Commands { get; set; }

        [Command("ping")]
        [AccessesUser(452597784516886538)]
        [Summary("Тестовая команда")]
        public async Task Pong()
        {
            await ReplyAsync("PONG!");
        }

        [Command("prefix")]
        [Accesses(Access.Admin)]
        [AccessesUser(452597784516886538)]
        [Summary("Команда для смены префикса")]
        public async Task ChangePrefix([Remainder] string prefix)
        {
            Console.WriteLine(prefix);

            GlobalData.Config.SetNewPrefix(Context.Guild.Id, prefix);
            GlobalData.Save();

            await ReplyAsync("Prefix changed to " + prefix);
        }

        [Command("outputs")]
        [Alias("print_next", "printNext")]
        [Summary("Командля для изменения параметра вывода следующей песни")]
        public async Task ChangeOutputNameSong(bool output)
        {
            GlobalData.Config.SetNewNeedOutput(Context.Guild.Id, output);
            GlobalData.Save();

            await ReplyAsync("Output name next song to " + output);
        }

        [Command("eval")]
        [Accesses(Access.Admin)]
        [Summary("Команда для выполнения кода во входной строке")]
        public async Task Evals([Remainder] string text)
        {
            await ReplyAsync(ExpressionEvaluator.Eval(text));
        }

        [Command("send")]
        [Accesses(Access.Admin)]
        [Summary("Команда для отправки сообщения от имени бота")]
        public async Task Send([Remainder] string text)
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(text);
        }

        [AccessesUser(452597784516886538)]
        public async Task AddAccess([Remainder] ulong userId)
        {
            await ReplyAsync("you do it");
        }


        [Command("relaod_conf")]
        [Alias("rc")]
        [Summary("Команда для перезагрузки файла с конфигами")]
        [AccessesUser(452597784516886538)]
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

            for (int i = 0; i < commands.Count; i++)
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

        [Command("dm")]
        [AccessesUser(452597784516886538)]
        [Summary("Команда для удаление последних n сообщений")]
        public async Task DeleteLastMessages([Remainder] int count = 1)
        {
            var messages = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();

            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await ReplyAsync("The messages have been deleted");
            await Task.Delay(3000);
            await message.DeleteAsync();
        }

        [Command("mdm")]
        [AccessesUser(452597784516886538)]
        [Summary("Команда для удаление последних n сообщений, отправленных более 2-х недель назад")]
        public async Task DeleteLastMessages2([Remainder] int count = 1)
        {
            var messages = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();

            foreach (var item in messages)
            {
                await item.DeleteAsync();
                await Task.Delay(1000);
            }

            
            var message = await ReplyAsync("The messages have been deleted");
            await Task.Delay(3000);
            await message.DeleteAsync();
        }

        [Command("cnt")]
        [AccessesUser(452597784516886538)]
        [Summary("Команда для получения количеста сообщений одного типа из последних 100")]
        public async Task CountLast()
        {
            var messages = (await Context.Channel.GetMessagesAsync(100).FlattenAsync()).ToList();

            var message = messages[0];
            messages.RemoveAt(0);
            await message.DeleteAsync();

            var typeMessToDelete = messages.ToList()[0];
            var cnt = 0;

            for (int i = 0; i < messages.Count; i++)
            {
                if (typeMessToDelete.Content == messages[i].Content)
                    cnt++;
            }

            await ReplyAsync($"Count: {cnt}");
        }

        [Command("bullying")]
        [Alias("bllng")]
        [AccessesUser(452597784516886538)]
        [Summary("Команда для удаления все сообщений из 100 последних в канале от пользователя")]
        public async Task Bullying([Remainder] IUser user)
        {
            var userId = user.Id;
            var messages = new List<IMessage>();

            foreach (var channel in Context.Guild.Channels)
            {
                messages.Clear();

                if (channel is SocketTextChannel)
                    (await (channel as ITextChannel).GetMessagesAsync(100).FlattenAsync()).ToList().ForEach(async x => {
                        if (x.Author.Id == userId)
                            if ((DateTimeOffset.Now - x.Timestamp).Days >= 13)
                            {
                                await x.DeleteAsync();
                                await Task.Delay(1000);
                            }
                            else
                                messages.Add(x);
                    });

                await (channel as SocketTextChannel).DeleteMessagesAsync(messages);
            }

            var message = await ReplyAsync("Bullying ended");
            await Task.Delay(1000);
            await message.DeleteAsync();
        }
    }

    public class ExpressionEvaluator
    {
        public static string Eval(string code)
        {
            CSharpCodeProvider codeProvider = new();
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
