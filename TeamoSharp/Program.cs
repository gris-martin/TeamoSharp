using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Globalization;
using System.Threading.Tasks;
using TeamoSharp.DataAccessLayer;
using TeamoSharp.Services;

namespace TeamoSharp
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var culture = new CultureInfo("en-SE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            var deps = new ServiceCollection()
                .AddLogging(ConfigureLogging)
                .AddSingleton(provider => new DiscordBot(provider.GetService<ILogger<DiscordBot>>()))
                .AddDbContext<TeamoContext>()
                .AddSingleton<IClientService, DiscordClientService>()
                .AddSingleton<IMainService, MainService>();

            var serviceProvider = deps.BuildServiceProvider();

            var bot = serviceProvider.GetService<DiscordBot>();
            bot.CreateCommands(serviceProvider);
            bot.CreateCallbacks(serviceProvider);

            await bot.ConnectAsync();

            await Task.Delay(-1);
        }

        public static void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddConsole(ConfigureConsole);
        }

        private static void ConfigureConsole(ConsoleLoggerOptions console)
        {
            console.IncludeScopes = true;
            console.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        }

        //public static IHostBuilder CreateHostBuilder(string[] args)
        //{
        //    return Host.CreateDefaultBuilder(args)
        //        .ConfigureServices((hostContext, services) =>
        //        {
        //            services.AddDbContext<TeamoContext>();

        //            services.AddLogging(ConfigureLogging);

        //            services.AddTransient<IClientService, DiscordService>();
        //            services.AddSingleton<IMainService, MainService>();

        //            services.AddSingleton<DiscordBot>();

        //            var serviceProvider = services.BuildServiceProvider();
        //            var bot = serviceProvider.GetRequiredService<DiscordBot>();
        //            bot.CreateCommands(serviceProvider);
        //            bot.CreateCallbacks(serviceProvider.GetRequiredService<IMainService>());
        //            bot.Client.ConnectAsync();
        //        });
        //}
    }
}
