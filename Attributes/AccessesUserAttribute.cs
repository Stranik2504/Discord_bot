using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Discord_Bot.Handlers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Discord_Bot.Attributes
{
    public class AccessesUserAttribute : PreconditionAttribute
    {
        private readonly ulong[] _userIds;

        public AccessesUserAttribute(params ulong[] userIds) => (_userIds, Group) = (userIds, "Access");

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as IGuildUser;

            if (_userIds != null && _userIds.Contains(user.Id))
                return await Task.FromResult(PreconditionResult.FromSuccess());

            return await Task.FromResult(PreconditionResult.FromError("You do not have permission"));
        }
    }
}
