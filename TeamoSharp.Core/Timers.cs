using System;
using System.Threading.Tasks;
using System.Timers;

namespace TeamoSharp
{
    public class Timers
    {
        public event EventHandler<ElapsedEventArgs> Update;
        public event EventHandler<ElapsedEventArgs> StartTimeReached;

        private DateTime EndDate { get; set; }

        public Timers(double updateInterval, DateTime endDate, Func<Task> updateActionAsync, Func<Task> endActionAsync)
        {
            _semaphore = new System.Threading.SemaphoreSlim(1, 1);

            // Update timer
            UpdateTimer = new Timer
            {
                Interval = updateInterval,
                AutoReset = true
            };
            UpdateTimer.Elapsed += async (sender, e) =>
            {
                Console.WriteLine("[TIMER] Updating");
                await _semaphore.WaitAsync();
                try
                {
                    await updateActionAsync();
                }
                finally
                {
                    _semaphore.Release();
                }
            };


            // Start timer
            EndDate = endDate;
            StartTimer = new Timer
            {
                AutoReset = false
            };
            StartTimer.Elapsed += async (sender, e) =>
            {
                Console.WriteLine("[TIMER] Finished");
                await _semaphore.WaitAsync();
                try
                {
                    UpdateTimer.Stop();
                    await endActionAsync();
                }
                finally
                {
                    _semaphore.Release();
                }
            };

        }

        public void ChangeEndDate(DateTime date)
        {
            double timeLeftMs = (date - DateTime.Now).TotalMilliseconds;
            // TODO: Exception/Error handling
            if (timeLeftMs <= 0)
                throw new Exception("Cannot change start to a time before now!");

            StartTimer.Interval = timeLeftMs;
        }

        public Timer UpdateTimer { get; private set; }
        public Timer StartTimer { get; private set; }

        private readonly System.Threading.SemaphoreSlim _semaphore;

        internal void Start()
        {
            StartTimer.Interval = (EndDate - DateTime.Now).TotalMilliseconds;
            StartTimer.Start();
            UpdateTimer.Start();
        }

        internal void Stop()
        {
            UpdateTimer.Stop();
            StartTimer.Stop();
        }
    }
}
