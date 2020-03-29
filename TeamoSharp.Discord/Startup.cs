using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeamoSharp.DataAccessLayer;
using TeamoSharp.Services;
using static TeamoSharp.Utils.LoggerCreationUtils;

namespace TeamoSharp.Discord
{
    public static class Startup
    {
        public static async Task CreateBotServicesAsync()
        {

            var bot = new DiscordBot();

            var deps = new ServiceCollection()
                .AddLogging(ConfigureLogging)
                .AddDbContext<TeamoContext>()
                .AddSingleton<IClientService, DiscordClientService>()
                .AddSingleton<IMainService, MainService>()
                .AddSingleton(bot.Client);

            var serviceProvider = deps.BuildServiceProvider();

            bot.CreateCommands(serviceProvider);
            bot.CreateCallbacks(serviceProvider);

            await bot.ConnectAsync();
        }
    }
}
