using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Discord_Bot.Handlers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Discord_Bot.Attributes
{
    public class AccessesAttribute : PreconditionAttribute
    {
        private readonly Access[] _accesses;

        public AccessesAttribute(params Access[] accesses) => (_accesses, Group) = (accesses, "Access");

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as IGuildUser;

            if (_accesses != null && _accesses.Contains((Access)GlobalData.Config.GetPremission(user.Id)))
                return await Task.FromResult(PreconditionResult.FromSuccess());

            return await Task.FromResult(PreconditionResult.FromError("You do not have permission"));
        }
    }
}
