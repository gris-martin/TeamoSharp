using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Timers;
using TeamoSharp.DataAccessLayer;
using TeamoSharp.Entities;
using TeamoSharp.Services;

namespace TeamoSharp.Core
{
    public class Timers
    {
        private readonly Timer _updateTimer;
        private readonly Timer _startTimer;
        private readonly TeamoEntry _entry;
        private readonly ILogger _logger;
        private readonly System.Threading.SemaphoreSlim _semaphore;
        private readonly TeamoContext _context;
        private readonly IClientService _clientService;

        public event EventHandler<ElapsedEventArgs> TimerFinished;

        public Timers(double updateInterval, TeamoEntry entry, TeamoContext context, IClientService clientService, ILogger logger)
        {
            _context = context;
            _clientService = clientService;
            _entry = entry;
            _logger = logger;

            _semaphore = new System.Threading.SemaphoreSlim(1, 1);
            var entryId = _entry.Id.Value;

            // Update timer
            _updateTimer = new Timer
            {
                Interval = updateInterval,
                AutoReset = true
            };
            _updateTimer.Elapsed += async (sender, e) =>
            {
                Console.WriteLine("[TIMER] Updating");
                await _semaphore.WaitAsync();
                try
                {
                    var dbEntry = _context.GetEntry(entryId);
                    await _clientService.UpdateMessageAsync(dbEntry);
                }
                finally
                {
                    _semaphore.Release();
                }
            };


            // Start timer
            _startTimer = new Timer
            {
                AutoReset = false
            };
            _startTimer.Elapsed += async (sender, e) =>
            {
                Console.WriteLine("[TIMER] Finished");
                _updateTimer.Stop();
                await _semaphore.WaitAsync();
                try
                {
                    _logger.LogInformation($"Creating start message for entry {entryId}");
                    var dbEntry = _context.GetEntry(entryId);
                    await _clientService.DeleteMessageAsync(entry.Message);
                    await _clientService.CreateStartMessageAsync(dbEntry);
                    await _context.DeleteAsync(entry.Id.Value);
                    EventHandler<ElapsedEventArgs> handler = TimerFinished;
                    handler?.Invoke(sender, e);
                    _logger.LogInformation($"Successfully created start message for {entryId}");
                }
                finally
                {
                    _semaphore.Release();
                }
            };

        }


        public async Task EditDateAsync(DateTime date)
        {
            // TODO: Better exception
            await _semaphore.WaitAsync();
            try
            {
                if (date <= DateTime.Now)
                    throw new Exception($"Cannot change to a date and time before now! Current date: {DateTime.Now}. Desired date: {date}");
                _startTimer.Interval = (date - DateTime.Now).TotalMilliseconds;
                var entry = await _context.EditDateAsync(date, _entry.Id.Value);
                await _clientService.UpdateMessageAsync(entry);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task EditMaxPlayersAsync(int numPlayers)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (numPlayers < 2)
                    throw new Exception($"Invalid number of players. The number of players must be between 2 and {int.MaxValue}");
                var post = await _context.EditNumPlayersAsync(numPlayers, _entry.Id.Value);
                await _clientService.UpdateMessageAsync(post);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task EditGameAsync(string game)
        {
            await _semaphore.WaitAsync();
            try
            {
                // TODO: Specify max length of game name somewhere
                if (game.Length > 40)
                    throw new Exception($"Game name too long ({game.Length} characters)! Maximum number of characters is 40");
                var post = await _context.EditGameAsync(game, _entry.Id.Value);
                await _clientService.UpdateMessageAsync(post);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddMemberAsync(Member member)
        {
            await _semaphore.WaitAsync();
            try
            {
                var post = await _context.AddMemberAsync(member, _entry.Message);
                await _clientService.UpdateMessageAsync(post);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        //Task RemoveMemberAsync(string userId, ClientMessage message);

        internal void Start()
        {
            _startTimer.Interval = (_entry.EndDate - DateTime.Now).TotalMilliseconds;
            _startTimer.Start();
            _updateTimer.Start();
        }

        public async Task StopAsync()
        {
            _updateTimer.Stop();
            _startTimer.Stop();
            await _semaphore.WaitAsync();
            try
            {
                var post = _context.GetEntry(_entry.Id.Value);
                await _clientService.DeleteMessageAsync(post.Message);
                await _context.DeleteAsync(_entry.Id.Value);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
