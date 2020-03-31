using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamoSharp.DataAccessLayer;

namespace TeamoSharp.Services
{

    public class MainService : IMainService
    {
        private readonly ILogger _logger;
        private readonly TeamoContext _dbContext;
        private readonly IClientService _clientService;

        private readonly IDictionary<int, Timers> _timers;

        public MainService(ILogger<MainService> logger, TeamoContext dbContext, IClientService discordService)
        {
            _logger = logger;
            _dbContext= dbContext;
            _clientService = discordService;
            _timers = new Dictionary<int, Timers>();
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


            // Create timers
            var entryId = entry.Id.Value;
            double updateInterval = 5000.0;
            var endDate = entry.EndDate;
            async Task updateFunc()
            {
                var dbEntry = _dbContext.GetEntry(entryId);
                await _clientService.UpdateMessageAsync(dbEntry);
            }
            async Task startFunc()
            {
                _logger.LogInformation($"Creating start message for entry {entryId}");
                var dbEntry = _dbContext.GetEntry(entryId);
                await _clientService.CreateStartMessageAsync(dbEntry);
                await DeleteAsync(entryId);
                _logger.LogInformation($"Successfully created start message for {entryId}");
            }
            var timers = new Timers(updateInterval, endDate, updateFunc, startFunc);
            timers.Start();
            _timers.Add(entryId, timers);
        }

        public async Task DeleteAsync(int postId)
        {
            var timersHolder = _timers[postId];
            timersHolder.Stop();
            _timers.Remove(postId);

            var post = _dbContext.GetEntry(postId);
            await _clientService.DeleteMessageAsync(post.Message);
            await _dbContext.DeleteAsync(postId);
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

        public async Task EditMaxPlayersAsync(int numPlayers, int postId)
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
