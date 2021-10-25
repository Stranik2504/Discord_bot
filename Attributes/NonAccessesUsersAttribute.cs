using System;
using Discord;
using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Attributes
{
    public class NonAccessesUsersAttribute : PreconditionAttribute
    {
        private ulong[] _userIds;

        public NonAccessesUsersAttribute(params ulong[] userIds) => _userIds = userIds.ToArray();

        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as IGuildUser;

            if (_userIds != null && _userIds.Contains(user.Id))
                return PreconditionResult.FromError("You do not have permission");

            return PreconditionResult.FromSuccess();
        }
    }
}