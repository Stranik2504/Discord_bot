using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class LoggingService
    {
        public static async Task LogAsync(string src, LogSeverity severity, string message, Exception exception = null)
        {
            if (severity.Equals(null)) severity = LogSeverity.Warning;

            await Append($"{GetSeverityString(severity)}", GetConsoleColor(severity));
            await Append($" [{SourceToString(src)}] ", ConsoleColor.DarkGray);

            if (!string.IsNullOrWhiteSpace(message)) await Append($"{message}\n", ConsoleColor.White);
            else if (exception == null) await Append("Uknown Exception. Exception Returned Null.\n", ConsoleColor.DarkRed);
            else if (exception.Message == null) await Append($"Unknownk \n{exception.StackTrace}\n", GetConsoleColor(severity));
            else await Append($"{exception.Message ?? "Unknownk"}\n{exception.StackTrace ?? "Unknown"}\n", GetConsoleColor(severity));
        }

        public static async Task LogCriticalAsync(string source, string message, Exception exc = null) => await LogAsync(source, LogSeverity.Critical, message, exc);

        public static async Task LogInformationAsync(string source, string message) => await LogAsync(source, LogSeverity.Info, message);

        private static async Task Append(string message, ConsoleColor color)
        {
            await Task.Run(() => {
                Console.ForegroundColor = color;
                Console.Write(message);
            });
        }

        private static string SourceToString(string src) => src.ToLower() switch
        {
            "lavanode_0_socket" => "lavanode_socket",
            "lavanode_0" => "lavenode",
            _ => src,
        };

        private static string GetSeverityString(LogSeverity severity) => severity.ToString();

        private static ConsoleColor GetConsoleColor(LogSeverity severity) => severity switch
        {
            LogSeverity.Critical => ConsoleColor.Red,
            LogSeverity.Debug => ConsoleColor.Magenta,
            LogSeverity.Error => ConsoleColor.DarkRed,
            LogSeverity.Info => ConsoleColor.Green,
            LogSeverity.Verbose => ConsoleColor.DarkCyan,
            LogSeverity.Warning => ConsoleColor.Yellow,
            _ => ConsoleColor.White,
        };
    }
}
