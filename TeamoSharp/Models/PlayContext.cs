using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TeamoSharp.Models
{
    public class PlayContext : DbContext
    {
        private readonly ILogger<PlayContext> _logger;

        public DbSet<PlayPost> Posts { get; set; }

        public PlayContext(ILogger<PlayContext> logger)
        {
            _logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=teamo.db");
        }

        public async Task<PlayPost> AddMemberAsync(ulong userId, int numPlayers, ulong messageId, ulong channelId)
        {
            var post = Posts.Single((a) => (ulong)a.DiscordChannelId == channelId && (ulong)a.DiscordMessageId == messageId);
            if (post.Members.Exists((a) => (ulong)a.DiscordUserId == userId))
            {
                _logger.LogWarning($"Trying to add member {userId} to database, but member already exists. Update numPlayers instead.");
                post.Members.Single((a) => (ulong)a.DiscordUserId == userId).NumPlayers = numPlayers;
            }
            else
            {
                post.Members.Add(new PlayMember()
                {
                    DiscordUserId = (long)userId,
                    NumPlayers = numPlayers
                });
            }
            await SaveChangesAsync();
            return post;
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
            var entry = await Posts.AddAsync(post);
            await SaveChangesAsync();
            _logger.LogDebug($"Database entry {channelId} : {messageId} created. Post id: {entry.Entity.PlayPostId}");
            return entry.Entity;
        }

        public async Task DeleteAsync(int postId)
        {
            _logger.LogDebug($"Deleting database entry {postId}...");
            var post = GetPost(postId);
            Posts.Remove(post);
            await SaveChangesAsync();
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
            var post = Posts.Single(a => a.PlayPostId == postId);
            return post;
        }

        public Task<PlayPost> RemoveMemberAsync(ulong userId, int numPlayers, ulong messageId, ulong channelId)
        {
            throw new NotImplementedException();
        }
    }
}
