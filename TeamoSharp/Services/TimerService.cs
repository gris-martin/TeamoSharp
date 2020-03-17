using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Timers;

namespace TeamoSharp.Services
{
    public interface ITimerService
    {
        public bool ChangeTime(ulong id, DateTime newDate);
        public bool AddTimer(ulong id, DateTime finishDate, Action timedFunc);
        public void RemoveTimer(ulong id);
    }

    public class TimerService : ITimerService
    {
        private readonly ILogger<TimerService> _logger;
        private readonly IDictionary<ulong, Timer> _timers;

        public TimerService(ILogger<TimerService> logger)
        {
            _timers = new Dictionary<ulong, Timer>();
            _logger = logger;
        }

        private Timer GetTimer(ulong id) => _timers[id];
        public bool ChangeTime(ulong id, DateTime date)
        {
            var timer = GetTimer(id);
            if (date <= DateTime.Now)
            {
                timer.Interval = (date - DateTime.Now).TotalMilliseconds;
            }
            return true;
        }

        public bool AddTimer(ulong id, DateTime date, Action timedFunc)
        {
            if (date <= DateTime.Now)
            {
                return false;
            }
            var timer = new Timer((date - DateTime.Now).TotalMilliseconds);
            timer.Elapsed += (sender, e) => timedFunc();
            timer.Elapsed += (sender, e) => _timers.Remove(id);
            timer.AutoReset = false;
            timer.Start();
            _timers.Add(id, timer);
            return true;
        }

        public void RemoveTimer(ulong id)
        {
            _timers.Remove(id);
        }
    }
}