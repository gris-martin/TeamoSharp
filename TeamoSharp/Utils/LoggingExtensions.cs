using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace TeamoSharp.Extensions
{
    public static class LoggingExtensions
    {
        public static void LogDSharp(this ILogger logger, object _, DebugLogMessageEventArgs e)
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
