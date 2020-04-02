using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamoSharp.Logging
{
    public class DiscordProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new DiscordLogger();
        }

        public void Dispose()
        {
            return;
        }
    }
}
