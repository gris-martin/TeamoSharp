using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TeamoSharp.DataAccessLayer.Models;

namespace TeamoSharp.Services
{
    public interface IDiscordService
    {
        Task<DiscordMessage> CreateMessageAsync(DateTime date, int numPlayers, string game, DiscordChannel channel);
        Task UpdateMessageAsync(Post post, DiscordChannel channel);
        Task DeleteMessageAsync(string messageId, DiscordChannel channel);
        Task CreateStartMessageAsync(Post post, DiscordChannel channel);
    }

    public class DiscordService : IDiscordService
    {
        private readonly ILogger _logger;

        public DiscordService(ILogger<DiscordService> logger)
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



        public async Task DeleteMessageAsync(string messageId, DiscordChannel channel)
        {
            ulong id = ulong.Parse(messageId);
            var message = await channel.GetMessageAsync(id);
            await channel.DeleteMessageAsync(message);
        }



        public async Task UpdateMessageAsync(Post post, DiscordChannel channel)
        {
            ulong id = ulong.Parse(post.Message.MessageId);
            var message = await channel.GetMessageAsync(id);
            await message.ModifyAsync(embed: CreateEmbed(post));
        }



        public Task CreateStartMessageAsync(Post post, DiscordChannel channel)
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
