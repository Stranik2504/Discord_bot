using Discord;
using Newtonsoft.Json;
using Discord_Bot.Structs;
using Discord_Bot.Services;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Discord_Bot.Handlers
{
    public class GlobalData
    {
        public static Config Config { get; set; }

        private static string _path = "Config.conf";

        public async static Task LoadAsync()
        {
            if (!File.Exists(_path))
            {
                Config = new();
                Save();

                await LoggingService.LogAsync("Bot", LogSeverity.Error, "No Config file found. A new one has been generated. Please close the & fill in the required section.");
                await Task.Delay(-1);
            }

            FileStream stream = File.Open(_path, FileMode.Open);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
            Config = (Config)xmlSerializer.Deserialize(stream);
        }

        public static void Save()
        {
            FileStream stream = File.Open(_path, FileMode.Create);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
            xmlSerializer.Serialize(stream, Config);
        }
    }
}
