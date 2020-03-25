﻿using DSharpPlus;
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
    public class DiscordBot
    {
        public DiscordClient Client { get; private set; }
        //public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        private readonly ILogger<DiscordBot> _logger;

        public DiscordBot(ILogger<DiscordBot> logger)
        {
            _logger = logger;

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
                if (e.Message.Content.ToLower(CultureInfo.CurrentCulture).StartsWith("ping", StringComparison.Ordinal))
                    await e.Message.RespondAsync("pong!").ConfigureAwait(false);
            };

            Client.MessageReactionAdded += async args =>
            {
                if (args.Emoji.IsNumberEmoji())
                {
                    Console.WriteLine($"Got number {args.Emoji.GetAsNumber()}");
                }
                else if (args.Emoji.IsCancelEmoji())
                {
                    Console.WriteLine("Get cancel emoji " + args.Emoji.Name + "!");
                }
            };

            Client.MessageReactionRemoved += async args =>
            {
            };

            Client.ConnectAsync();
        }

        public void CreateCommands(IServiceProvider services)
        {
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
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Info, "Teamo", "Successfully connected!", DateTime.Now);
            //_logger.LogInformation("Teamo connected successfully!");
            return Task.CompletedTask;
        }
    }
}