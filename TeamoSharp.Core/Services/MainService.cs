using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamoSharp.Core;
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
            var entry = _dbContext.GetEntry(message);
            await _timers[entry.Id.Value].AddMemberAsync(member);
        }

        public async Task<Entities.TeamoEntry> CreateAsync(Entities.TeamoEntry entry)
        {
            // Create Discord message
            var message = await _clientService.CreateMessageAsync(entry);
            entry.Message = message;

            // Create database entry
            try
            {
                entry = await _dbContext.CreateAsync(entry);
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
            double updateInterval = 5000.0;
            var timers = new Timers(updateInterval, entry, _dbContext, _clientService, _logger);
            timers.TimerFinished += (sender, e) => _timers.Remove(entry.Id.Value);
            timers.Start();
            _timers.Add(entry.Id.Value, timers);

            return entry;
        }

        public async Task DeleteAsync(int postId)
        {
            var timers = _timers[postId];
            await timers.StopAsync();
            _timers.Remove(postId);

        }

        public async Task EditDateAsync(DateTime date, int postId)
        {
            var timer = _timers[postId];
            await timer.EditDateAsync(date);
        }

        public async Task EditMaxPlayersAsync(int numPlayers, int postId)
        {
            var timer = _timers[postId];
            await timer.EditMaxPlayersAsync(numPlayers);
        }

        public async Task EditGameAsync(string game, int postId)
        {
            var timer = _timers[postId];
            await timer.EditGameAsync(game);
        }

        public Task RemoveMemberAsync(string userId, Entities.ClientMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
