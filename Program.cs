using Discord_Bot.Services;
using System;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public enum Access : uint { Admin = 1 }

    class Program
    {
        static async Task Main(string[] args) => await new BotService().Init();
    }
}
