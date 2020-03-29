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
            options.EnableSensitiveDataLogging();
        }

        public async Task<Entities.TeamoEntry> AddMemberAsync(Entities.Member member, Entities.ClientMessage message)
        {
            _logger.LogInformation("Adding member!\n" +
                $"message id: {message.MessageId}\n" +
                $"channel id: {message.ChannelId}\n" +
                $"server id: {message.ServerId}\n"
            );

            var post = Posts.Single(
                (a) => 
                    a.Message.MessageId == message.MessageId && 
                    a.Message.ChannelId == message.ChannelId && 
                    a.Message.ServerId == message.ServerId
            ); 

            if (post.Members.Exists((a) => a.ClientUserId == member.ClientUserId))
            {
                _logger.LogWarning($"Trying to add member {member.ClientUserId} to database, but member already exists. Updating numPlayers instead.");
                post.Members.Single((a) => a.ClientUserId == member.ClientUserId).NumPlayers = member.NumPlayers;
            }
            else
            {
                post.Members.Add(new Member()
                {
                    ClientUserId = member.ClientUserId,
                    NumPlayers = member.NumPlayers
                });
            }
            await SaveChangesAsync();
            return post.AsEntityType();
        }

        public async Task<Entities.TeamoEntry> CreateAsync(Entities.TeamoEntry entry)
        {
            if (entry.Id != null)
            {
                // TODO: Better exception
                throw new Exception("Cannot create a new database post with non-zero ID");
            }
            var post = await Posts.AddAsync(entry.AsModelType());
            await SaveChangesAsync();
            _logger.LogDebug($"Database entry " +
                $"{entry.Message.ChannelId} : " +
                $"{entry.Message.MessageId} : " +
                $"{entry.Message.ServerId} created. " +
                $"Post id: {post.Entity.PostId}");
            return post.Entity.AsEntityType();
        }

        public async Task DeleteAsync(int postId)
        {
            _logger.LogDebug($"Deleting database entry {postId}...");
            var post = GetEntry(postId);
            Posts.Remove(post.AsModelType());
            int status = await SaveChangesAsync();
            var numPosts = Posts.Count();
            _logger.LogDebug($"Database entry {postId} deleted. Status: {status}. Num posts: {numPosts}");
        }

        public async Task<Entities.TeamoEntry> EditDateAsync(DateTime date, int postId)
        {
            var post = GetEntry(postId);
            post.EndDate = date;
            await SaveChangesAsync();
            return post;
        }

        public async Task<Entities.TeamoEntry> EditGameAsync(string game, int postId)
        {
            var post = GetEntry(postId);
            post.Game = game;
            await SaveChangesAsync();
            return post;
        }

        public async Task<Entities.TeamoEntry> EditNumPlayersAsync(int numPlayers, int postId)
        {
            var post = GetEntry(postId);
            post.MaxPlayers = numPlayers;
            await SaveChangesAsync();
            return post;
        }

        public Entities.TeamoEntry GetEntry(int postId)
        {
            var post = Posts.Single(a => a.PostId == postId);
            return post.AsEntityType();
        }

        public Task<Post> RemoveMemberAsync(ulong userId, int numPlayers, ulong messageId, ulong channelId)
        {
            throw new NotImplementedException();
        }
    }
}
