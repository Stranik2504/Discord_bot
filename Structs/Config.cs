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
        public List<(ulong Id, ushort Volume)> VolumeByGuild { get; set; } = new();

        public string GetPrefix(ulong guildId)
        {
            foreach (var (Id, Prefix) in PrefixByGuild) { if (Id == guildId) return Prefix; }

            PrefixByGuild.Add((guildId, "!"));
            return "!";
        }

        public void SetNewPrefix(ulong guildId, string prefix)
        {
            GetPrefix(guildId);

            for (int i = 0; i < PrefixByGuild.Count; i++) { if (PrefixByGuild[i].Id == guildId) { PrefixByGuild[i] = (guildId, prefix); return; } }
        }

        public ushort GetVoulme(ulong guildId)
        {
            foreach (var (Id, Volume) in VolumeByGuild) { if (Id == guildId) return Volume; }

            VolumeByGuild.Add((guildId, 100));
            return 100;
        }

        public void SetNewVoulme(ulong guildId, ushort volume)
        {
            GetVoulme(guildId);

            for (int i = 0; i < VolumeByGuild.Count; i++) { if (VolumeByGuild[i].Id == guildId) { VolumeByGuild[i] = (guildId, volume); return; } }
        }
    }
}
