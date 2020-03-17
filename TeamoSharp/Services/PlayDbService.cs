using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamoSharp.Models;

namespace TeamoSharp.Services
{
    public interface IPlayDbService
    {
        Task<PlayPost> CreateAsync(DateTime date, int numPlayers, string game, ulong messageId, ulong channelId);
        Task<PlayPost> EditDateAsync(DateTime date, ulong messageId, ulong channelId);
        Task<PlayPost> EditNumPlayersAsync(int numPlayers, ulong messageId, ulong channelId);
        Task<PlayPost> EditGameAsync(string game, ulong messageId, ulong channelId);
        Task<PlayPost> AddMemberAsync(ulong userId, int numPlayers, ulong messageId, ulong channelId);
        PlayPost GetPost(int postId);
        Task DeleteAsync(int postId);
    }

    public class PlayDbService : IPlayDbService
    {
        private readonly ILogger _logger;

        public PlayDbService(ILogger<PlayDbService> logger)
        {
            _logger = logger;
        }

        public Task<PlayPost> AddMemberAsync(ulong userId, int numPlayers, ulong messageId, ulong channelId)
        {
            throw new NotImplementedException();
        }

        public async Task<PlayPost> CreateAsync(DateTime date, int numPlayers, string game, ulong messageId, ulong channelId)
        {
            _logger.LogDebug($"Creating new database entry. {date}; {numPlayers}; {game}; {messageId}; {channelId}");
            var post = new PlayPost
            {
                EndDate = date,
                MaxPlayers = numPlayers,
                Game = game,
                DiscordMessageId = (long)messageId,
                DiscordChannelId = (long)channelId
            };

            using var context = new PlayContext();
            var entry = await context.AddAsync(post);
            await context.SaveChangesAsync();
            post = entry.Entity;
            _logger.LogDebug($"Database entry {channelId} : {messageId} created. Post id: {post.PlayPostId}");
            return post;
        }

        public async Task DeleteAsync(int postId)
        {
            _logger.LogDebug($"Deleting database entry {postId}...");
            using var context = new PlayContext();
            var post = GetPost(postId);
            context.Remove(post);
            await context.SaveChangesAsync();
            _logger.LogDebug($"Database entry {postId} deleted.");
        }

        public Task<PlayPost> EditDateAsync(DateTime date, ulong messageId, ulong channelId)
        {
            throw new NotImplementedException();
        }

        public Task<PlayPost> EditGameAsync(string game, ulong messageId, ulong channelId)
        {
            throw new NotImplementedException();
        }

        public Task<PlayPost> EditNumPlayersAsync(int numPlayers, ulong messageId, ulong channelId)
        {
            throw new NotImplementedException();
        }

        public PlayPost GetPost(int postId)
        {
            using var context = new PlayContext();
            var post = context.Posts.Single(a => a.PlayPostId == postId);
            return post;
        }
    }
}
