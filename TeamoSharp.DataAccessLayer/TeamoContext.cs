using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeamoSharp.DataAccessLayer.Models;

namespace TeamoSharp.DataAccessLayer
{
    public class TeamoContext : DbContext
    {
        private readonly ILogger<TeamoContext> _logger;

        public DbSet<Post> Posts { get; set; }

        public TeamoContext(ILogger<TeamoContext> logger)
        {
            _logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=teamo.db");
        }

        public async Task<Post> AddMemberAsync(string userId, int numPlayers, string messageId, string channelId = null, string serverId = null)
        {


            var post = Posts.Single(
                (a) => 
                    a.Message.MessageId == messageId && 
                    a.Message.ChannelId == channelId && 
                    a.Message.ServerId == serverId
            ); 

            if (post.Members.Exists((a) => a.ClientUserId == userId))
            {
                _logger.LogWarning($"Trying to add member {userId} to database, but member already exists. Updating numPlayers instead.");
                post.Members.Single((a) => a.ClientUserId == userId).NumPlayers = numPlayers;
            }
            else
            {
                post.Members.Add(new Member()
                {
                    ClientUserId = userId,
                    NumPlayers = numPlayers
                });
            }
            await SaveChangesAsync();
            return post;
        }

        public async Task<Post> CreateAsync(Post post)
        {
            if (post.PostId != 0)
            {
                // TODO: Better exception
                throw new Exception("Cannot create a new database post with non-zero ID");
            }
            var entry = await Posts.AddAsync(post);
            await SaveChangesAsync();
            _logger.LogDebug($"Database entry " +
                $"{post.Message.ChannelId} : " +
                $"{post.Message.MessageId} : " +
                $"{post.Message.ServerId} created. " +
                $"Post id: {entry.Entity.PostId}");
            return entry.Entity;
        }

        public async Task<Post> CreateAsync(DateTime date, int numPlayers, string game, string messageId, string channelId = null, string serverId = null)
        {
            _logger.LogDebug($"Creating new database entry. {date}; {numPlayers}; {game}; {messageId}; {channelId}");
            var message = new ClientMessage
            {
                MessageId = messageId,
                ChannelId = channelId,
                ServerId = serverId
            };
            var post = new Post
            {
                EndDate = date,
                MaxPlayers = numPlayers,
                Game = game,
                Message = message
            };
            return await CreateAsync(post);
        }

        public async Task DeleteAsync(int postId)
        {
            _logger.LogDebug($"Deleting database entry {postId}...");
            var post = GetPost(postId);
            Posts.Remove(post);
            int status = await SaveChangesAsync();
            var numPosts = Posts.Count();
            _logger.LogDebug($"Database entry {postId} deleted. Status: {status}. Num posts: {numPosts}");
        }

        public Task<Post> EditDateAsync(DateTime date, ulong messageId, ulong channelId)
        {
            throw new NotImplementedException();
        }

        public Task<Post> EditGameAsync(string game, ulong messageId, ulong channelId)
        {
            throw new NotImplementedException();
        }

        public Task<Post> EditNumPlayersAsync(int numPlayers, ulong messageId, ulong channelId)
        {
            throw new NotImplementedException();
        }

        public Post GetPost(int postId)
        {
            var post = Posts.Single(a => a.PostId == postId);
            return post;
        }

        public Task<Post> RemoveMemberAsync(ulong userId, int numPlayers, ulong messageId, ulong channelId)
        {
            throw new NotImplementedException();
        }
    }
}
