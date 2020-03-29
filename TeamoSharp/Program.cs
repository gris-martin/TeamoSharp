using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Globalization;
using System.Threading.Tasks;
using TeamoSharp.DataAccessLayer;
using TeamoSharp.Services;
using static TeamoSharp.Utils.LoggerCreationUtils;

namespace TeamoSharp
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var culture = new CultureInfo("en-SE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            await Discord.Startup.CreateBotServicesAsync();

            await Task.Delay(-1);

            //CreateHostBuilder(args).Build().Run();
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
