using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamoSharp.Models;

namespace TeamoSharp.Services
{
    public interface IPlayDiscordService
    {
        Task<DiscordMessage> CreateMessageAsync(DateTime date, int numPlayers, string game, DiscordChannel channel);
        Task UpdateMessageAsync(PlayPost post, DiscordChannel channel);
        Task DeleteMessageAsync(ulong messageId, DiscordChannel channel);
        Task CreateStartMessageAsync(PlayPost post, DiscordChannel channel);
    }

    public class PlayDiscordService : IPlayDiscordService
    {
        private readonly ILogger _logger;

        public PlayDiscordService(ILogger<PlayDiscordService> logger)
        {
            _logger = logger;
        }

        public async Task<DiscordMessage> CreateMessageAsync(DateTime date, int numPlayers, string game, DiscordChannel channel)
        {
            _logger.LogInformation("Creating new Discord message");
            var embed = CreateEmbed(date, numPlayers, game);
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }



        public async Task DeleteMessageAsync(ulong messageId, DiscordChannel channel)
        {
            var message = await channel.GetMessageAsync(messageId);
            await channel.DeleteMessageAsync(message);
        }



        public async Task UpdateMessageAsync(PlayPost post, DiscordChannel channel)
        {
            var message = await channel.GetMessageAsync((ulong)post.DiscordMessageId);
            await message.ModifyAsync(embed: CreateEmbed(post));
        }



        public Task CreateStartMessageAsync(PlayPost post, DiscordChannel channel)
        {
            _logger.LogError("Method to create start message not implemented!");
            return Task.CompletedTask;
        }


        // ------------ Internal utility methods ------------
        static private DiscordEmbed CreateEmbed(PlayPost post)
        {
            return CreateEmbed(post.EndDate, post.MaxPlayers, post.Game, post.PlayPostId);
        }

        static private DiscordEmbed CreateEmbed(DateTime date, int numPlayers, string game, int? postId = null)
        {
            var builder = new DiscordEmbedBuilder
            {
                Description = "Hello from Teamo"
            };
            builder.AddField("Game", $"{game}");
            builder.AddField("NumPlayers", $"{numPlayers}");
            builder.AddField("End date", $"{date}");
            if (!(postId is null))
            {
                builder.AddField("Post id", $"{postId}");
            }
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Last updated {DateTime.Now}"
            };
            return builder.Build();
        }
    }
}
