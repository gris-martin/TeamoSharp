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
            bool isUpdating = false;
            var entryId = _entry.Id.Value;

            // Update timer
            _updateTimer = new Timer
            {
                Interval = updateInterval,
                AutoReset = true
            };
            _updateTimer.Elapsed += async (sender, e) =>
            {
                if (isUpdating)
                    return;
                Console.WriteLine($"[TIMER] Updating {entryId}");
                await ExclusiveAsync(async () =>
                {
                    var dbEntry = _context.GetEntry(entryId);
                    await _clientService.UpdateMessageAsync(dbEntry);
                });
                Console.WriteLine($"[TIMER] Update done {entryId}");
            };


            // Start timer
            _startTimer = new Timer
            {
                AutoReset = false
            };
            _startTimer.Elapsed += async (sender, e) =>
            {
                Console.WriteLine($"[TIMER] Starting finish {entryId}");
                _updateTimer.Stop();
                await ExclusiveAsync(async () =>
                {
                    _logger.LogInformation($"Creating start message for entry {entryId}");
                    var dbEntry = _context.GetEntry(entryId);
                    await _clientService.DeleteMessageAsync(entry.Message);
                    await _clientService.CreateStartMessageAsync(dbEntry);
                    await _context.DeleteAsync(entry.Id.Value);
                    EventHandler<ElapsedEventArgs> handler = TimerFinished;
                    handler?.Invoke(sender, e);
                    _logger.LogInformation($"Successfully created start message for {entryId}");
                });
                Console.WriteLine($"[TIMER] Finished {entryId}");
            };

        }

        private async Task ExclusiveAsync(Func<Task> funcAsync)
        {
            await _semaphore.WaitAsync();
            try
            {
                await funcAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal async Task EditDateAsync(DateTime date)
        {
            await ExclusiveAsync(async () =>
            {
                if (date <= DateTime.Now)
                    throw new Exception($"Cannot change to a date and time before now! Current date: {DateTime.Now}. Desired date: {date}");
                _startTimer.Interval = (date - DateTime.Now).TotalMilliseconds;
                var entry = await _context.EditDateAsync(date, _entry.Id.Value);
                await _clientService.UpdateMessageAsync(entry);
            });
        }

        internal async Task EditMaxPlayersAsync(int numPlayers)
        {
            await ExclusiveAsync(async () =>
            {
                if (numPlayers < 2)
                    throw new Exception($"Invalid number of players. The number of players must be between 2 and {int.MaxValue}");
                var post = await _context.EditNumPlayersAsync(numPlayers, _entry.Id.Value);
                await _clientService.UpdateMessageAsync(post);

            });
        }

        internal async Task EditGameAsync(string game)
        {
            await ExclusiveAsync(async () =>
            {
                // TODO: Specify max length of game name somewhere
                if (game.Length > 40)
                    throw new Exception($"Game name too long ({game.Length} characters)! Maximum number of characters is 40");
                var post = await _context.EditGameAsync(game, _entry.Id.Value);
                await _clientService.UpdateMessageAsync(post);
            });
        }

        internal async Task AddMemberAsync(Member member)
        {
            await ExclusiveAsync(async () =>
            {
                var post = await _context.AddMemberAsync(member, _entry.Message);
                await _clientService.UpdateMessageAsync(post);
            });
        }
        //Task RemoveMemberAsync(string userId, ClientMessage message);

        internal void Start()
        {
            _startTimer.Interval = (_entry.EndDate - DateTime.Now).TotalMilliseconds;
            _startTimer.Start();
            _updateTimer.Start();
        }

        internal async Task StopAsync()
        {
            _updateTimer.Stop();
            _startTimer.Stop();
            await ExclusiveAsync(async () =>
            {
                var post = _context.GetEntry(_entry.Id.Value);
                await _clientService.DeleteMessageAsync(post.Message);
                await _context.DeleteAsync(_entry.Id.Value);
            });
        }
    }
}
