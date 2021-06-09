using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Structs
{
    public class Config
    {
        public string Token { get; set; }
        public string GameStatus { get; set; } = default;
        public List<(ulong Id, string Prefix)> PrefixByGuild { get; set; } = new();

        public string GetPrefix(ulong guildId)
        {
            foreach (var item in PrefixByGuild) { if (item.Id == guildId) return item.Prefix; }

            PrefixByGuild.Add((guildId, "!"));
            return "!";
        }

        public void SetNewPrefix(ulong guildId, string prefix)
        {
            GetPrefix(guildId);

            for (int i = 0; i < PrefixByGuild.Count; i++) { if (PrefixByGuild[i].Id == guildId) { PrefixByGuild[i] = (guildId, prefix); return; } }
        }
    }
}
