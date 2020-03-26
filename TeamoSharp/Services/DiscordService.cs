using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamoSharp.DataAccessLayer.Models;
using TeamoSharp.Utils;
using static TeamoSharp.Utils.DiscordEmojiUtils;

namespace TeamoSharp.Services
{
    public interface IClientService
    {
        Task<DiscordMessage> CreateMessageAsync(DateTime date,
                                                int numPlayers,
                                                string game,
                                                string channelId,
                                                string serverId = null);
        Task UpdateMessageAsync(Post post);
        Task DeleteMessageAsync(Post post);
        Task CreateStartMessageAsync(Post post);
    }

    public class DiscordService : IClientService
    {
        private readonly ILogger _logger;
        private readonly DiscordClient _client;

        public DiscordService(ILogger<DiscordService> logger, DiscordBot bot)
        {
            _logger = logger;
            _client = bot.Client;
        }

        public async Task<DiscordMessage> CreateMessageAsync(DateTime date,
                                                             int numPlayers,
                                                             string game,
                                                             string channelId,
                                                             string serverId = null)
        {
            _logger.LogInformation("Creating new Discord message");
            var embed = await CreateEmbedAsync(date, numPlayers, game);
            var channel = await _client.GetChannelAsync(ulong.Parse(channelId));
            var message = await channel.SendMessageAsync(embed: embed);

            //var numReactions = numPlayers < 11 ? numPlayers : 10;
            //for (int i = 1; i < numReactions; i++)
            //{
            //    await message.CreateReactionAsync(CreateEmojiFromNumber(i));
            //}
            //await message.CreateReactionAsync(GetCancelEmoji());

            return message;
        }



        public async Task DeleteMessageAsync(Post post)
        {
            ulong messageId = ulong.Parse(post.Message.MessageId);
            ulong channelId = ulong.Parse(post.Message.ChannelId);
            var channel = await _client.GetChannelAsync(channelId);
            var message = await channel.GetMessageAsync(messageId);
            await channel.DeleteMessageAsync(message);
        }



        public async Task UpdateMessageAsync(Post post)
        {
            ulong messageId = ulong.Parse(post.Message.MessageId);
            ulong channelId = ulong.Parse(post.Message.ChannelId);
            var channel = await _client.GetChannelAsync(channelId);
            var message = await channel.GetMessageAsync(messageId);
            var embed = await CreateEmbedAsync(post);
            await message.ModifyAsync(embed: embed);

            var maxPlayers = post.MaxPlayers;
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

                foreach (var member in post.Members)
                {
                    ulong memberId = ulong.Parse(member.ClientUserId);
                    if (emojiUsers.Any((u) => u.Id == memberId))
                    {
                        if (emojiNumber != member.NumPlayers)
                        {
                            await message.DeleteReactionAsync(emoji, emojiUsers.Single((u) => u.Id == memberId));
                        }
                    } else if (emojiNumber == member.NumPlayers)
                    {
                        _logger.LogWarning($"Mismatch between number of players in database and discord reactions for member {memberId}." +
                            $"Number of players according to database: {member.NumPlayers}");
                    }
                }
            }

            // TODO: Update number of emojis
        }



        public Task CreateStartMessageAsync(Post post)
        {
            _logger.LogError("Method to create start message not implemented!");
            return Task.CompletedTask;
        }


        // ------------ Internal utility methods ------------
        private async Task<DiscordEmbed> CreateEmbedAsync(Post post)
        {
            return await CreateEmbedAsync(post.EndDate, post.MaxPlayers, post.Game, post.Members, post.PostId);
        }

        private async Task<DiscordEmbed> CreateEmbedAsync(DateTime date, int numPlayers, string game, IEnumerable<Member> members = null, int? postId = null)
        {
            var builder = new DiscordEmbedBuilder
            {
                Title = $"Dags för **{game}**!!",
                Description = $"**Start: {date}** - För anmälan, tryck emotes nedan med antal som vill spela."
            };
            builder.Color = DiscordColor.Purple;
            builder.AddField("Tid kvar", $"{date - DateTime.Now}");
            builder.AddField("Spelare per lag", $"{numPlayers}");
            builder.AddField("Anmälda", $"Inga anmälda än");

            if (members != null)
            {
                foreach (var member in members)
                {
                    var discordUser = await _client.GetUserAsync(ulong.Parse(member.ClientUserId));
                    builder.AddField($"Member: {discordUser.Username}", $"{member.NumPlayers}");
                }
            }
            if (!(postId is null))
            {
                builder.AddField("Id", $"{postId}");
            }
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Uppdateras var 15:e sekund. Senast uppdaterad {DateTime.Now}"
            };
            return builder.Build();
        }
    }
}
