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
        public List<(ulong Id, string Prefix, ushort Volume, bool OutputNameSongs)> ParamsByGuild { get; set; } = new();

        public string GetPrefix(ulong guildId)
        {
            foreach (var (Id, Prefix, _, _) in ParamsByGuild) { if (Id == guildId) return Prefix; }

            ParamsByGuild.Add((guildId, "!", 100, false));
            return "!";
        }

        public void SetNewPrefix(ulong guildId, string prefix)
        {
            GetPrefix(guildId);

            for (int i = 0; i < ParamsByGuild.Count; i++) { if (ParamsByGuild[i].Id == guildId) { ParamsByGuild[i] = (guildId, prefix, ParamsByGuild[i].Volume, ParamsByGuild[i].OutputNameSongs); return; } }
        }

        public ushort GetVoulme(ulong guildId)
        {
            foreach (var (Id, _, Volume, _) in ParamsByGuild) { if (Id == guildId) return Volume; }

            ParamsByGuild.Add((guildId, "!", 100, false));
            return 100;
        }

        public void SetNewVoulme(ulong guildId, ushort volume)
        {
            GetVoulme(guildId);

            for (int i = 0; i < ParamsByGuild.Count; i++) { if (ParamsByGuild[i].Id == guildId) { ParamsByGuild[i] = (guildId, ParamsByGuild[i].Prefix, volume, ParamsByGuild[i].OutputNameSongs); return; } }
        }

        public bool GetNeedOutput(ulong guildId)
        {
            foreach (var (Id, _, _, OutputNameSongs) in ParamsByGuild) { if (Id == guildId) return OutputNameSongs; }

            ParamsByGuild.Add((guildId, "!", 100, false));
            return false;
        }

        public void SetNewNeedOutput(ulong guildId, bool outputNameSongs)
        {
            GetNeedOutput(guildId);

            for (int i = 0; i < ParamsByGuild.Count; i++) { if (ParamsByGuild[i].Id == guildId) { ParamsByGuild[i] = (guildId, ParamsByGuild[i].Prefix, ParamsByGuild[i].Volume, outputNameSongs); return; } }
        }
    }
}
