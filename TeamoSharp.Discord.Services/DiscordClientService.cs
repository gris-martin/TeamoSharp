using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamoSharp.Discord.Utils.Utils;
//using TeamoSharp.DataAccessLayer.Models;
using TeamoSharp.Utils;
using static TeamoSharp.Utils.DiscordEmojiUtils;

namespace TeamoSharp.Services
{
    public class DiscordClientService : IClientService
    {
        private readonly ILogger _logger;
        private readonly DiscordClient _client;

        public DiscordClientService(ILogger<DiscordClientService> logger, DiscordBot bot)
        {
            _logger = logger;
            _client = bot.Client;
        }

        public async Task<Entities.ClientMessage> CreateMessageAsync(Entities.TeamoEntry entry)
        {
            _logger.LogInformation("Creating new Discord message");
            var embed = await CreateEmbedAsync(entry);
            var channel = await _client.GetChannelAsync(ulong.Parse(entry.Message.ChannelId));
            var dMessage = await channel.SendMessageAsync(embed: embed);

            //var numReactions = numPlayers < 11 ? numPlayers : 10;
            //for (int i = 1; i < numReactions; i++)
            //{
            //    await message.CreateReactionAsync(CreateEmojiFromNumber(i));
            //}
            //await message.CreateReactionAsync(GetCancelEmoji());

            return dMessage.AsEntityType();
        }



        public async Task DeleteMessageAsync(Entities.ClientMessage message)
        {
            ulong messageId = ulong.Parse(message.MessageId);
            ulong channelId = ulong.Parse(message.ChannelId);
            var channel = await _client.GetChannelAsync(channelId);
            try
            {
                var discordMessage = await channel.GetMessageAsync(messageId);
                await discordMessage.DeleteAsync();
            }
            catch (DSharpPlus.Exceptions.NotFoundException nfe)
            {
                _logger.LogWarning(nfe, "Tried deleting a message that had already been deleted.");
            }
        }



        public async Task UpdateMessageAsync(Entities.TeamoEntry entry)
        {
            ulong messageId = ulong.Parse(entry.Message.MessageId);
            ulong channelId = ulong.Parse(entry.Message.ChannelId);
            var channel = await _client.GetChannelAsync(channelId);
            var message = await channel.GetMessageAsync(messageId);
            var embed = await CreateEmbedAsync(entry);
            await message.ModifyAsync(embed: embed);

            var maxPlayers = entry.MaxPlayers;
            //_logger.LogInformation($"Number of reactions: {message.Reactions.Count}");
            //var reactions = message.Reactions;
            //var reaction = reactions.First();

            for (int i = 0; i < NumberEmojiNames.Length; i++)
            {
                var emoji = CreateEmojiFromNumber(i + 1);
                var emojiUsers = await message.GetReactionsAsync(CreateEmojiFromNumber(i + 1));
                var emojiNumber = emoji.GetAsNumber();
                if (i < maxPlayers)
                {
                    if (emojiUsers.Count == 0)
                    {
                        await message.CreateReactionAsync(emoji);
                    }
                    else if (!emojiUsers.Contains(_client.CurrentUser))
                    {
                        _logger.LogWarning($"Number emoji {emoji} exists without bot reaction. Removing and readding emoji.");
                        await message.DeleteReactionsEmojiAsync(emoji);
                        await message.CreateReactionAsync(emoji);
                    }
                }

                if (i > maxPlayers - 1 && emojiUsers.Count > 0)
                {
                    await message.DeleteReactionsEmojiAsync(emoji);
                }

                foreach (var member in entry.Members)
                {
                    ulong memberId = ulong.Parse(member.ClientUserId);
                    if (emojiUsers.Any((u) => u.Id == memberId))
                    {
                        if (emojiNumber != member.NumPlayers)
                        {
                            await message.DeleteReactionAsync(emoji, emojiUsers.Single((u) => u.Id == memberId));
                        }
                    }
                    else if (emojiNumber == member.NumPlayers)
                    {
                        _logger.LogWarning($"Mismatch between number of players in database and discord reactions for member {memberId}." +
                            $"Number of players according to database: {member.NumPlayers}");
                    }
                }
            }

            // TODO: Update number of emojis
        }



        public Task CreateStartMessageAsync(Entities.TeamoEntry entry)
        {
            _logger.LogError("Method to create start message not implemented!");
            return Task.CompletedTask;
        }


        // ------------ Internal utility methods ------------
        private async Task<DiscordEmbed> CreateEmbedAsync(Entities.TeamoEntry entry)
        {
            var game = entry.Game;
            var date = entry.EndDate;
            var maxPlayers = entry.MaxPlayers;
            var members = entry.Members;
            var id = entry.Id;

            var builder = new DiscordEmbedBuilder
            {
                Title = $"Dags för **{game}**!!",
                Description = $"**Start: {date}** - För anmälan, tryck emotes nedan med antal som vill spela."
            };
            builder.Color = DiscordColor.Purple;
            builder.AddField("Tid kvar", $"{date - DateTime.Now}");
            builder.AddField("Spelare per lag", $"{maxPlayers}");
            builder.AddField("Anmälda", $"Inga anmälda än");

            if (members != null)
            {
                foreach (var member in members)
                {
                    var discordUser = await _client.GetUserAsync(ulong.Parse(member.ClientUserId));
                    builder.AddField($"Member: {discordUser.Username}", $"{member.NumPlayers}");
                }
            }
            if (!(id is null))
            {
                builder.AddField("Id", $"{id}");
            }
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Uppdateras var 15:e sekund. Senast uppdaterad {DateTime.Now}"
            };
            return builder.Build();
        }
    }
}
