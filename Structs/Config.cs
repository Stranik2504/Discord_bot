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
        public List<(ulong Id, string Prefix, ushort Volume, bool OutputNameSongs, Repeate Repeate)> ParamsByGuild { get; set; }
        public List<(ulong Id, ulong Premission)> Accesses { get; set; } = new() { (452597784516886538, 1) };
        public List<ulong> NonAccessUsers { get; set; }

        public string GetPrefix(ulong guildId) => (string)GetParam(guildId, "prefix");

        public void SetNewPrefix(ulong guildId, string prefix) => SetParam(guildId, "prefix", prefix);

        public ushort GetVoulme(ulong guildId) => (ushort)GetParam(guildId, "volume");

        public void SetNewVoulme(ulong guildId, ushort volume) => SetParam(guildId, "volume", volume);

        public bool GetNeedOutput(ulong guildId) => (bool)GetParam(guildId, "output");

        public void SetNewNeedOutput(ulong guildId, bool outputNameSongs) => SetParam(guildId, "output", outputNameSongs);

        public Repeate GetRepeate(ulong guildId) => (Repeate)GetParam(guildId, "repeate");

        public void SetNewRepeate(ulong guildId, ulong repeate) => SetParam(guildId, "repeate", (Repeate)repeate);

        public ulong GetPremission(ulong userId)
        {
            foreach (var (Id, Premission) in Accesses)
            {
                if (Id == userId)
                    return Premission;
            }

            return 0;
        }

        private object GetParam(ulong guildId, string nameParam)
        {
            foreach (var (Id, Prefix, Volume, OutputNameSongs, Repeate) in ParamsByGuild)
                if (Id == guildId)
                    return nameParam.ToLower() switch
                    {
                        "prefix" => Prefix,
                        "volume" => Volume,
                        "output" => OutputNameSongs,
                        "repeate" => Repeate,
                        _ => Prefix
                    };

            ParamsByGuild.Add((guildId, (string)GetDefault("prefix"), (ushort)GetDefault("volume"), (bool)GetDefault("output"), (Repeate)GetDefault("repeate")));

            return GetDefault(nameParam);
        }

        private void SetParam(ulong guildId, string nameParam, object inParam)
        {
            GetParam(guildId, nameParam);

            for (int i = 0; i < ParamsByGuild.Count; i++) 
            { 
                if (ParamsByGuild[i].Id == guildId) 
                {
                    var param = ParamsByGuild[i];
                    ParamsByGuild[i] = (guildId, 
                        nameParam == "prefix" ? (string)inParam : param.Prefix,
                        nameParam == "volume" ? (ushort)inParam : param.Volume,
                        nameParam == "output" ? (bool)inParam : param.OutputNameSongs,
                        nameParam == "repeate" ? (Repeate)inParam : param.Repeate); 
                    return; 
                } 
            }
        }

        private static object GetDefault(string nameParam) => nameParam.ToLower() switch
        {
            "prefix" => "!",
            "volume" => 100,
            "output" => false,
            "repeate" => Repeate.None,
            _ => "!"
        };
    }
}
