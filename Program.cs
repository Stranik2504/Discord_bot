using Discord_Bot.Services;
using System;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public enum Access : ulong { Admin = 1 }
    public enum Repeate : uint { None = 0, Single = 1, All = 2 }

    class Program
    {
        static async Task Main() => await new BotService().Init();
    }
}
