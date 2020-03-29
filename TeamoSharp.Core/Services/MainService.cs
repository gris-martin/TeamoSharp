using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using TeamoSharp.DataAccessLayer;

namespace TeamoSharp.Services
{
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
        private readonly IClientService _clientService;

        private readonly IDictionary<int, TimersHolder> _timers;

        public MainService(ILogger<MainService> logger, TeamoContext dbContext, IClientService discordService)
        {
            _logger = logger;
            _dbContext= dbContext;
            _clientService = discordService;
            _timers = new Dictionary<int, TimersHolder>();
        }

        public async Task AddMemberAsync(Entities.Member member, Entities.ClientMessage message)
        {
            var post = await _dbContext.AddMemberAsync(member, message);
            await _clientService.UpdateMessageAsync(post);
        }

        public async Task CreateAsync(Entities.TeamoEntry entry)
        {
            // Create Discord message
            var message = await _clientService.CreateMessageAsync(entry);
            entry.Message = message;

            // Create database entry
            try
            {
                _logger.LogInformation($"Creating new entry: {message.ServerId}; {message.ChannelId}; {message.MessageId}");
                entry = await _dbContext.CreateAsync(entry);
                _logger.LogInformation($"New entry created: {message.ServerId}; {message.ChannelId}; {message.MessageId}");
                await _clientService.UpdateMessageAsync(entry);
            }
            catch
            {
                await _clientService.DeleteMessageAsync(message);
                throw;
            }

            if (entry.Id == null)
            {
                // TODO: Error handling
                throw new Exception("Newly created Discord message got a null Id!");
            }

            // Create update timer
            var updateTimer = new Timer(5000);
            updateTimer.Elapsed += async (sender, e) =>
            {
                _logger.LogDebug($"Updating {entry.Id}");
                var dbEntry = _dbContext.GetEntry(entry.Id.Value);
                _logger.LogDebug($"Got dbPost {entry.Id}");
                await _clientService.UpdateMessageAsync(dbEntry);
                _logger.LogDebug($"Updating done {entry.Id}");
            };
            updateTimer.AutoReset = true;
            updateTimer.Start();

            // Create start timer
            var startTimer = new Timer((entry.EndDate - DateTime.Now).TotalMilliseconds);
            startTimer.Elapsed += async (sender, e) =>
            {
                _logger.LogDebug($"Timer done! {entry.Id}");
                var dbPost = _dbContext.GetEntry(entry.Id.Value);
                _logger.LogDebug($"Got dbentry {entry.Id}");
                await _clientService.CreateStartMessageAsync(dbPost);
                _logger.LogDebug($"Start message created {entry.Id}");
                await DeleteAsync(dbPost.Id.Value);
                _logger.LogDebug($"Entry deleted {entry.Id}");
            };
            startTimer.AutoReset = false;
            startTimer.Start();

            var timersHolder = new TimersHolder(updateTimer, startTimer);
            _timers.Add(entry.Id.Value, timersHolder);
        }

        public async Task DeleteAsync(int postId)
        {
            _logger.LogDebug($"Deleting entry and timers");
            var timersHolder = _timers[postId];
            timersHolder.UpdateTimer.Stop();
            timersHolder.StartTimer.Stop();
            _timers.Remove(postId);
            _logger.LogDebug($"Timers deleted");

            var post = _dbContext.GetEntry(postId);
            _logger.LogDebug($"Got dbentry");
            await _clientService.DeleteMessageAsync(post.Message);
            _logger.LogDebug($"Deleted client message");
            await _dbContext.DeleteAsync(postId);
            _logger.LogDebug($"Deleted dbentry");
        }

        public async Task EditDateAsync(DateTime date, int postId)
        {
            // TODO: Better exception
            if (date <= DateTime.Now)
                throw new Exception($"Cannot change to a date and time before now! Current date: {DateTime.Now}. Desired date: {date}");
            _timers[postId].StartTimer.Interval = (date - DateTime.Now).TotalMilliseconds;
            var entry = await _dbContext.EditDateAsync(date, postId);
            await _clientService.UpdateMessageAsync(entry);
        }

        public async Task EditGameAsync(string game, int postId)
        {
            if (game.Length > 40)
                throw new Exception($"Game name too long ({game.Length} characters)! Maximum number of characters is 40");
            var post = await _dbContext.EditGameAsync(game, postId);
            await _clientService.UpdateMessageAsync(post);
        }

        public async Task EditMaxPlayresAsync(int numPlayers, int postId)
        {
            if (numPlayers < 2)
                throw new Exception($"Invalid number of players. The number of players must be between 2 and {int.MaxValue}");
            var post = await _dbContext.EditNumPlayersAsync(numPlayers, postId);
            await _clientService.UpdateMessageAsync(post);
        }

        public Task RemoveMemberAsync(string userId, Entities.ClientMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
