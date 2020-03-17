using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using TeamoSharp.ErrorHandling;
using TeamoSharp.Models;

namespace TeamoSharp.Services
{
    public interface IPlayService
    {
        Task CreateAsync(DateTime date, int numPlayers, string game, ulong channelId, DiscordClient client);
        Task EditDateAsync(DateTime date, ulong messageId, DiscordChannel channel);
        Task EditNumPlayersAsync(int numPlayers, ulong messageId, DiscordChannel channel);
        Task EditGameAsync(string game, ulong messageId, DiscordChannel channel);
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

    public class PlayService : IPlayService
    {
        private readonly ILogger _logger;
        private readonly IPlayDbService _dbService;
        private readonly IPlayDiscordService _discordService;

        private readonly IDictionary<int, TimersHolder> _timers;

        public PlayService(ILogger<PlayService> logger, IPlayDbService dbService, IPlayDiscordService discordService)
        {
            _logger = logger;
            _dbService = dbService;
            _discordService = discordService;
            _timers = new Dictionary<int, TimersHolder>();
        }

        public async Task CreateAsync(DateTime date, int numPlayers, string game, ulong channelId, DiscordClient client)
        {
            // Create Discord message
            var channel = await client.GetChannelAsync(channelId);
            var message = await _discordService.CreateMessageAsync(date, numPlayers, game, channel);

            // Create database entry
            PlayPost post = null;
            try
            {
                post = await _dbService.CreateAsync(date, numPlayers, game, message.Id, channel.Id);
                _logger.LogInformation($"New entry created: {channel.Id} : {message.Id}");
                await _discordService.UpdateMessageAsync(post, channel);
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
                _logger.LogInformation("Updating!");
                var dbPost = _dbService.GetPost(post.PlayPostId);
                await _discordService.UpdateMessageAsync(dbPost, channel);
            };
            updateTimer.AutoReset = true;
            updateTimer.Start();

            // Create start timer
            var startTimer = new Timer((date - DateTime.Now).TotalMilliseconds);
            startTimer.Elapsed += async (sender, e) =>
            {
                var dbPost = _dbService.GetPost(post.PlayPostId);
                await _discordService.CreateStartMessageAsync(dbPost, channel);
                await DeleteAsync(dbPost.PlayPostId, client);
            };
            startTimer.AutoReset = false;
            startTimer.Start();
            var timersHolder = new TimersHolder(updateTimer, startTimer);
            _timers.Add(post.PlayPostId, timersHolder);
        }

        public async Task DeleteAsync(int postId, DiscordClient client)
        {
            var post = _dbService.GetPost(postId);

            var timersHolder = _timers[postId];
            timersHolder.UpdateTimer.Stop();
            timersHolder.StartTimer.Stop();
            _timers.Remove(postId);

            var channel = await client.GetChannelAsync((ulong)post.DiscordChannelId);
            await _discordService.DeleteMessageAsync((ulong)post.DiscordMessageId, channel);

            await _dbService.DeleteAsync(postId);
        }

        public Task EditDateAsync(DateTime date, ulong messageId, DiscordChannel channel)
        {
            _logger.LogError("Method not implemented!");
            return Task.CompletedTask;
        }

        public Task EditGameAsync(string game, ulong messageId, DiscordChannel channel)
        {
            _logger.LogError("Method not implemented!");
            return Task.CompletedTask;
        }

        public Task EditNumPlayersAsync(int numPlayers, ulong messageId, DiscordChannel channel)
        {
            _logger.LogError("Method not implemented!");
            return Task.CompletedTask;
        }
    }
}
