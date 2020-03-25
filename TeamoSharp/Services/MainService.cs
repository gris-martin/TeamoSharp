using DSharpPlus;
using DSharpPlus.Entities;
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
        Task CreateAsync(DateTime date, int numPlayers, string game, ulong channelId, DiscordClient client);
        Task EditDateAsync(DateTime date, int postId, DiscordChannel channel);
        Task EditNumPlayersAsync(int numPlayers, int postId, DiscordChannel channel);
        Task EditGameAsync(string game, int postId, DiscordChannel channel);
        Task DeleteAsync(int postId, DiscordClient client);
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

        public async Task CreateAsync(DateTime date, int numPlayers, string game, ulong channelId, DiscordClient client)
        {
            // Create Discord message
            var channel = await client.GetChannelAsync(channelId);
            var message = await _discordService.CreateMessageAsync(date, numPlayers, game, channelId.ToString());

            // Create database entry
            Post post = null;
            try
            {
                post = await _dbContext.CreateAsync(date, numPlayers, game, message.Id.ToString(), channel.Id.ToString());
                _logger.LogInformation($"New entry created: {channel.Id} : {message.Id}");
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
                await DeleteAsync(dbPost.PostId, client);
            };
            startTimer.AutoReset = false;
            startTimer.Start();
            var timersHolder = new TimersHolder(updateTimer, startTimer);
            _timers.Add(post.PostId, timersHolder);
        }

        public async Task DeleteAsync(int postId, DiscordClient client)
        {
            var post = _dbContext.GetPost(postId);

            var timersHolder = _timers[postId];
            timersHolder.UpdateTimer.Stop();
            timersHolder.StartTimer.Stop();
            _timers.Remove(postId);

            ulong channelId = ulong.Parse(post.Message.ChannelId);
            var channel = await client.GetChannelAsync(channelId);
            await _discordService.DeleteMessageAsync(post);

            await _dbContext.DeleteAsync(postId);
        }

        public async Task EditDateAsync(DateTime date, int postId, DiscordChannel channel)
        {
            // TODO: Better exception
            if (date <= DateTime.Now)
                throw new Exception($"Cannot change to a date and time before now! Current date: {DateTime.Now}. Desired date: {date}");
            _timers[postId].StartTimer.Interval = (date - DateTime.Now).TotalMilliseconds;
            var post = await _dbContext.EditDateAsync(date, postId);
            await _discordService.UpdateMessageAsync(post);
        }

        public async Task EditGameAsync(string game, int postId, DiscordChannel channel)
        {
            if (game.Length > 40)
                throw new Exception($"Game name too long ({game.Length} characters)! Maximum number of characters is 40");
            var post = await _dbContext.EditGameAsync(game, postId);
            await _discordService.UpdateMessageAsync(post);
        }

        public async Task EditNumPlayersAsync(int numPlayers, int postId, DiscordChannel channel)
        {
            if (numPlayers < 2)
                throw new Exception($"Invalid number of players. The number of players must be between 2 and {int.MaxValue}");
            var post = await _dbContext.EditNumPlayersAsync(numPlayers, postId);
            await _discordService.UpdateMessageAsync(post);
        }
    }
}
