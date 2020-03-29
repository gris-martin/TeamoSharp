using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;
using TeamoSharp.Discord.Utils.Utils;
using TeamoSharp.Services;
using TeamoSharp.Utils;
using static TeamoSharp.Utils.LoggerCreationUtils;


namespace TeamoSharp
{
    public class DiscordBot
    {
        public DiscordClient Client { get; private set; }
        //public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        private readonly ILogger _logger;

        public DiscordBot()
        {
            var loggerFactory = LoggerFactory.Create(ConfigureLogging);
            _logger = loggerFactory.CreateLogger<DiscordBot>();

            _logger.LogWarning("Creating DiscordBot");

            var botToken = Environment.GetEnvironmentVariable("TEAMO_BOT_TOKEN");
            if (botToken is null)
            {
                _logger.LogError("No bot token specified! Set the TEAMO_BOT_TOKEN environment to allow connecting the bot.");
                Environment.Exit(-1);
            }

            var config = new DiscordConfiguration
            {
                Token = Environment.GetEnvironmentVariable("TEAMO_BOT_TOKEN"),
                UseInternalLogHandler = false
            };

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;
            Client.DebugLogger.LogMessageReceived += _logger.LogDSharp;

            Client.MessageCreated += async e =>
            {
                _logger.LogInformation("A new message was created!");
                if (e.Message.Content.ToLower(CultureInfo.CurrentCulture).StartsWith("ping", StringComparison.Ordinal))
                    await e.Message.RespondAsync("pong!").ConfigureAwait(false);
            };
            _logger.LogWarning("DiscordBot created");
        }

        public void CreateCommands(IServiceProvider services)
        {
            _logger.LogWarning("Creating commands");
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
            _logger.LogWarning("Commands created");
        }

        public void CreateCallbacks(IServiceProvider services)
        {
            var mainService = services.GetRequiredService<IMainService>();

            Client.MessageReactionAdded += async args =>
            {
                if (args.User.IsCurrent)
                    return;

                var emoji = args.Emoji;
                if (emoji.IsNumberEmoji())
                {
                    var numPlayers = emoji.GetAsNumber();
                    var member = new Entities.Member
                    {
                        ClientUserId = args.User.Id.ToString(),
                        NumPlayers = numPlayers
                    };
                    var message = args.Message.AsEntityType();
                    await mainService.AddMemberAsync(member, message);
                }
                else if (args.Emoji.IsCancelEmoji())
                {
                    Console.WriteLine("Get cancel emoji " + args.Emoji.Name + "!");
                }
            };

            Client.MessageReactionRemoved += async args =>
            {
            };
        }

        public async Task ConnectAsync()
        {
            await Client.ConnectAsync();
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Info, "Teamo", "Successfully connected!", DateTime.Now);
            //_logger.LogInformation("Teamo connected successfully!");
            return Task.CompletedTask;
        }
    }
}