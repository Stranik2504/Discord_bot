using System;
using Discord;
using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Attributes
{
    public class NonAccessesUsersAttribute(params ulong[] userIds) : PreconditionAttribute
    {
        private readonly ulong[] _userIds = userIds.ToArray();

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as IGuildUser;

            if (_userIds != null && _userIds.Contains(user.Id))
                return Task.FromResult(PreconditionResult.FromError("You do not have permission"));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}