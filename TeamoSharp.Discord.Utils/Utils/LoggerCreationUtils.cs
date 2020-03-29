using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamoSharp.Utils
{
    public static class LoggerCreationUtils
    {
        public static void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddConsole(ConfigureConsole);
            //logging.AddProvider(new Logging.DiscordProvider());
        }

        private static void ConfigureConsole(ConsoleLoggerOptions console)
        {
            console.IncludeScopes = true;
            console.TimestampFormat = "yyyy-MM-dd HH:mm ";
        }
    }
}
