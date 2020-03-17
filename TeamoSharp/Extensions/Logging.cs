using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamoSharp.Extensions
{
    public static class Logging
    {
        public static void LogDSharp(this ILogger logger, object sender, DebugLogMessageEventArgs e)
        {
            switch (e.Level)
            {
                case DSharpPlus.LogLevel.Debug:
                    logger.LogDebug(e.Exception, e.Message);
                    break;
                case DSharpPlus.LogLevel.Info:
                    logger.LogInformation(e.Exception, e.Message);
                    break;
                case DSharpPlus.LogLevel.Warning:
                    logger.LogWarning(e.Exception, e.Message);
                    break;
                case DSharpPlus.LogLevel.Error:
                    logger.LogError(e.Exception, e.Message);
                    break;
                case DSharpPlus.LogLevel.Critical:
                    logger.LogCritical(e.Exception, e.Message);
                    break;
                default:
                    break;
            }
        }
    }
}
