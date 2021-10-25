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
        private ulong[] _userIds;
        private Access[] _accesses;

        public AccessesAttribute(params object[] input)
        {
            var userIds = new List<ulong>();
            var accesses = new List<Access>();

            foreach (var item in input)
            {
                switch (item)
                {
                    case ulong or uint or long or int:
                        userIds.Add(ulong.Parse(item.ToString()));
                        break;
                    case Access:
                        accesses.Add((Access)item);
                        break;
                }
            }

            _userIds = userIds.ToArray();
            _accesses = accesses.ToArray();
        }

        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as IGuildUser;

            if (_userIds != null && _userIds.Contains(user.Id))
                return await Task.FromResult(PreconditionResult.FromSuccess());

            if (_accesses != null && _accesses.Contains((Access)GlobalData.Config.GetPremission(user.Id)))
                return await Task.FromResult(PreconditionResult.FromSuccess());

            return await Task.FromResult(PreconditionResult.FromError("You do not have permission"));
        }
    }
}
