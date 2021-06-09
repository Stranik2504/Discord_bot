using Discord_Bot.Services;
using System;
using System.Threading.Tasks;

namespace Discord_Bot
{
    class Program
    {
        static async Task Main(string[] args) => await new BotService().Init();
    }
}
