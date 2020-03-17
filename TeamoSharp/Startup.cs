using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using TeamoSharp.Services;

namespace TeamoSharp
{
    class Startup
    {
        [SuppressMessage("Design", "ASP0000:Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'", Justification = "<Pending>")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Models.PlayContext>();

            services.AddLogging((logging) =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddConsole((console) =>
                {
                    console.IncludeScopes = true;
                    console.TimestampFormat = "yyyy-MM-dd HH:mm ";
                });
                //logging.AddProvider(new Logging.DiscordProvider());
            });

            services.AddScoped      <   ITeamoService,          TeamoService        >();
            services.AddSingleton   <   ITimerService,          TimerService        >();

            services.AddTransient   <   IPlayDiscordService,    PlayDiscordService  >();
            services.AddTransient   <   IPlayDbService,         PlayDbService       >();
            services.AddSingleton   <   IPlayService,           PlayService         >();

            var serviceProvider = services.BuildServiceProvider();
            var bot = new Bot(serviceProvider);
            services.AddSingleton(bot);
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }
}
