using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;
using TeamoSharp.Extensions;

namespace TeamoSharp
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        //public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        private readonly ILogger<Bot> _logger;

        public Bot(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            _logger = scope.ServiceProvider.GetService<ILogger<Bot>>();

            var json = string.Empty;

            //using (var fs = File.OpenRead("config.json"))
            //using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            //    json = sr.ReadToEnd();

            //var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = "NjU3ODk2MjExODkzODQ2MDI2.XmaVYQ.za8ITDGb7TC2ZyYlKYH3FZ8OZpI",
                UseInternalLogHandler = false
            };

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;
            Client.DebugLogger.LogMessageReceived += _logger.LogDSharp;

            //Client.UseInteractivity(new InteractivityConfiguration
            //{
            //    Timeout = TimeSpan.FromMinutes(2)
            //});

            var commandsConfig = new CommandsNextConfiguration
            {
                //StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                DmHelp = true,
                Services = services
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<Commands.TeamoCommands>();
            //Commands.RegisterCommands<FunCommands>();
            //Commands.RegisterCommands<ItemCommands>();
            //Commands.RegisterCommands<ProfileCommands>();
            //Commands.RegisterCommands<TeamCommands>();

            Client.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower(CultureInfo.CurrentCulture).StartsWith("ping", StringComparison.Ordinal))
                    await e.Message.RespondAsync("pong!").ConfigureAwait(false);
            };

            Client.ConnectAsync();
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Info, "Teamo", "Successfully connected!", DateTime.Now);
            //_logger.LogInformation("Teamo connected successfully!");
            return Task.CompletedTask;
        }
    }
}