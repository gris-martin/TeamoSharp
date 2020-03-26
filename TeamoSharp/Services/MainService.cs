using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using TeamoSharp.DataAccessLayer;
using TeamoSharp.DataAccessLayer.Models;

namespace TeamoSharp.Services
{
    public interface IMainService
    {
        Task CreateAsync(DateTime date, int numPlayers, string game, string channelId, string serverId);
        Task EditDateAsync(DateTime date, int postId);
        Task EditNumPlayersAsync(int numPlayers, int postId);
        Task EditGameAsync(string game, int postId);
        Task DeleteAsync(int postId);
        Task AddMemberAsync(string userId, string channelId, string serverId, string messageId, int numPlayers);
        Task RemoveMemberAsync(string userId, string channelId, string serverId, string messageId);
    }

    public class TimersHolder
    {
        public TimersHolder(Timer updateTimer, Timer startTimer)
        {
            UpdateTimer = updateTimer;
            StartTimer = startTimer;
        }

        public Timer UpdateTimer { get; private set; }
        public Timer StartTimer { get; private set; }
    }

    public class MainService : IMainService
    {
        private readonly ILogger _logger;
        private readonly TeamoContext _dbContext;
        private readonly IClientService _discordService;

        private readonly IDictionary<int, TimersHolder> _timers;

        public MainService(ILogger<MainService> logger, TeamoContext dbContext, IClientService discordService)
        {
            _logger = logger;
            _dbContext= dbContext;
            _discordService = discordService;
            _timers = new Dictionary<int, TimersHolder>();
        }

        public async Task AddMemberAsync(string userId, string channelId, string serverId, string messageId, int numPlayers)
        {
            var post = await _dbContext.AddMemberAsync(userId, numPlayers, messageId, channelId, serverId);
            await _discordService.UpdateMessageAsync(post);
        }

        public async Task CreateAsync(DateTime date, int numPlayers, string game, string channelId, string serverId)
        {
            // Create Discord message
            var message = await _discordService.CreateMessageAsync(date, numPlayers, game, channelId, serverId);

            // Create database entry
            Post post = null;
            try
            {
                post = await _dbContext.CreateAsync(date, numPlayers, game, message.Id.ToString(), channelId, serverId);
                _logger.LogInformation($"New entry created: {channelId} : {message.Id}");
                await _discordService.UpdateMessageAsync(post);
            }
            catch (Exception e)
            {
                await message.DeleteAsync();
                throw e;
            }

            // Create update timer
            var updateTimer = new Timer(5000);
            updateTimer.Elapsed += async (sender, e) =>
            {
                var dbPost = _dbContext.GetPost(post.PostId);
                await _discordService.UpdateMessageAsync(dbPost);
            };
            updateTimer.AutoReset = true;
            updateTimer.Start();

            // Create start timer
            var startTimer = new Timer((date - DateTime.Now).TotalMilliseconds);
            startTimer.Elapsed += async (sender, e) =>
            {
                var dbPost = _dbContext.GetPost(post.PostId);
                await _discordService.CreateStartMessageAsync(dbPost);
                await DeleteAsync(dbPost.PostId);
            };
            startTimer.AutoReset = false;
            startTimer.Start();
            var timersHolder = new TimersHolder(updateTimer, startTimer);
            _timers.Add(post.PostId, timersHolder);
        }

        public async Task DeleteAsync(int postId)
        {
            var post = _dbContext.GetPost(postId);

            var timersHolder = _timers[postId];
            timersHolder.UpdateTimer.Stop();
            timersHolder.StartTimer.Stop();
            _timers.Remove(postId);

            await _discordService.DeleteMessageAsync(post);

            await _dbContext.DeleteAsync(postId);
        }

        public async Task EditDateAsync(DateTime date, int postId)
        {
            // TODO: Better exception
            if (date <= DateTime.Now)
                throw new Exception($"Cannot change to a date and time before now! Current date: {DateTime.Now}. Desired date: {date}");
            _timers[postId].StartTimer.Interval = (date - DateTime.Now).TotalMilliseconds;
            var post = await _dbContext.EditDateAsync(date, postId);
            await _discordService.UpdateMessageAsync(post);
        }

        public async Task EditGameAsync(string game, int postId)
        {
            if (game.Length > 40)
                throw new Exception($"Game name too long ({game.Length} characters)! Maximum number of characters is 40");
            var post = await _dbContext.EditGameAsync(game, postId);
            await _discordService.UpdateMessageAsync(post);
        }

        public async Task EditNumPlayersAsync(int numPlayers, int postId)
        {
            if (numPlayers < 2)
                throw new Exception($"Invalid number of players. The number of players must be between 2 and {int.MaxValue}");
            var post = await _dbContext.EditNumPlayersAsync(numPlayers, postId);
            await _discordService.UpdateMessageAsync(post);
        }

        public Task RemoveMemberAsync(string userId, string channelId, string serverId, string messageId)
        {
            throw new NotImplementedException();
        }
    }
}
