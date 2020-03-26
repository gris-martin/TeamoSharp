using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;
using TeamoSharp.Services;
using TeamoSharp.Utils;

namespace TeamoSharp
{
    public class DiscordBot
    {
        public DiscordClient Client { get; private set; }
        //public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        private readonly ILogger<DiscordBot> _logger;

        public DiscordBot(ILogger<DiscordBot> logger)
        {
            _logger = logger;
            _logger.LogWarning("Creating DiscordBot");

            var json = string.Empty;

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

        public void CreateCallbacks(IMainService mainService)
        {
            Client.MessageReactionAdded += async args =>
            {
                if (args.User.IsCurrent)
                    return;

                var emoji = args.Emoji;
                if (emoji.IsNumberEmoji())
                {
                    var numPlayers = emoji.GetAsNumber();
                    await mainService.AddMemberAsync(args.User.Id.ToString(),
                                                     args.Channel.Id.ToString(),
                                                     args.Guild.Id.ToString(),
                                                     args.Message.Id.ToString(),
                                                     numPlayers);
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

        private Task OnClientReady(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Info, "Teamo", "Successfully connected!", DateTime.Now);
            //_logger.LogInformation("Teamo connected successfully!");
            return Task.CompletedTask;
        }
    }
}