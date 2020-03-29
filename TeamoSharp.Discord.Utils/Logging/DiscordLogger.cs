using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamoSharp.Logging
{
    public class DiscordLogger : ILogger
    {
        public DiscordChannel Channel { get; set; } = null;

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return !(Channel is null);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var strBuilder = new StringBuilder($"{DateTime.Now}");
            strBuilder.Append("[");
            switch (logLevel)
            {
                case LogLevel.Trace:
                    strBuilder.Append("TRACE");
                    break;
                case LogLevel.Debug:
                    strBuilder.Append("DEBUG");
                    break;
                case LogLevel.Information:
                    strBuilder.Append("INFO");
                    break;
                case LogLevel.Warning:
                    strBuilder.Append("WARN");
                    break;
                case LogLevel.Error:
                    strBuilder.Append("ERROR");
                    break;
                case LogLevel.Critical:
                    strBuilder.Append("CRIT");
                    break;
                default:
                    strBuilder.Append("NONE");
                    break;
            }
            strBuilder.Append("]");
            strBuilder.Append($" {formatter(state, exception)}");

            // TODO: Create queue
            Task.Run(async () => await Channel?.SendMessageAsync($"{DateTime.Now} "));
        }
    }
}
