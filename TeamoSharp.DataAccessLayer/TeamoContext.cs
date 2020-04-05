using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeamoSharp.DataAccessLayer.Models;

namespace TeamoSharp.DataAccessLayer
{
    public class TeamoContext : DbContext
    {
        private readonly ILogger<TeamoContext> _logger;
        private readonly SemaphoreSlim _semaphore;

        public DbSet<Post> Posts { get; set; }

        public TeamoContext(ILogger<TeamoContext> logger)
        {
            _semaphore = new SemaphoreSlim(1, 1);
            _logger = logger;
        }

        public TeamoContext(ILogger<TeamoContext> logger, DbContextOptions<TeamoContext> options) : base(options)
        {
            _semaphore = new SemaphoreSlim(1, 1);
            _logger = logger;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite("Data Source=teamo.db");
            }
            options.EnableSensitiveDataLogging();
        }

        private async Task<TResult> ExclusiveAsync<TResult>(Func<Task<TResult>> funcAsync)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await funcAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Entities.TeamoEntry> AddMemberAsync(Entities.Member member, Entities.ClientMessage message)
        {
            return await ExclusiveAsync(async () =>
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
            });
        }

        public async Task<Entities.TeamoEntry> CreateAsync(Entities.TeamoEntry entry)
        {
            return await ExclusiveAsync(async () =>
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
            });
        }

        public async Task DeleteAsync(int postId)
        {
            await _semaphore.WaitAsync();
            try
            {
                var post = GetPost(postId);
                Posts.Remove(post);
                await SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Entities.TeamoEntry> EditDateAsync(DateTime date, int postId)
        {
            return await ExclusiveAsync(async () =>
            {
                var post = GetPost(postId);
                post.EndDate = date;
                await SaveChangesAsync();
                return post.AsEntityType();
            });
        }

        public async Task<Entities.TeamoEntry> EditGameAsync(string game, int postId)
        {
            return await ExclusiveAsync(async () =>
            {
                var post = GetPost(postId);
                post.Game = game;
                await SaveChangesAsync();
                return post.AsEntityType();
            });
        }

        public async Task<Entities.TeamoEntry> EditNumPlayersAsync(int numPlayers, int postId)
        {
            return await ExclusiveAsync(async () =>
            {
                var post = GetPost(postId);
                post.MaxPlayers = numPlayers;
                await SaveChangesAsync();
                return post.AsEntityType();
            });
        }

        public Entities.TeamoEntry GetEntry(int postId)
        {
            _semaphore.Wait();
            try
            {
                var post = Posts.Single(a => a.PostId == postId);
                return post.AsEntityType();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Entities.TeamoEntry GetEntry(Entities.ClientMessage message)
        {
            _semaphore.Wait();
            try
            {
                var post = Posts.Single(
                    (a) =>
                        a.Message.MessageId == message.MessageId &&
                        a.Message.ChannelId == message.ChannelId &&
                        a.Message.ServerId == message.ServerId
                );
                return post.AsEntityType();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private Post GetPost(int postId)
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
