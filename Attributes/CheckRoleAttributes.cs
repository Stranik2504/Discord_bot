using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Discord_Bot.Attributes
{
    public class CheckRole : PreconditionAttribute
    {
        private List<string> _roles;

        public CheckRole(params string[] roles)
        {
            _roles = roles.ToList();
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as IGuildUser;
            var discordRoles = context.Guild.Roles.Where(gr => _roles.Any(r => gr.Name == r));

            foreach (var role in discordRoles)
            {
                var userInRole = user.RoleIds.Any(ri => ri == role.Id);

                if (userInRole)
                {
                    return await Task.FromResult(PreconditionResult.FromSuccess());
                }
            }

            return await Task.FromResult(PreconditionResult.FromError("You do not have permission to use this role."));
        }
    }
}
