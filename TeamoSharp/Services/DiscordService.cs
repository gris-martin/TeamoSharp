using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TeamoSharp.DataAccessLayer.Models;

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
            var embed = CreateEmbed(date, numPlayers, game);
            var channel = await _client.GetChannelAsync(ulong.Parse(channelId));
            var message = await channel.SendMessageAsync(embed: embed);
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
            await message.ModifyAsync(embed: CreateEmbed(post));
        }



        public Task CreateStartMessageAsync(Post post)
        {
            _logger.LogError("Method to create start message not implemented!");
            return Task.CompletedTask;
        }


        // ------------ Internal utility methods ------------
        static private DiscordEmbed CreateEmbed(Post post)
        {
            return CreateEmbed(post.EndDate, post.MaxPlayers, post.Game, post.PostId);
        }

        static private DiscordEmbed CreateEmbed(DateTime date, int numPlayers, string game, int? postId = null)
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
