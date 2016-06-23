using System;

namespace PhoenixSocket
{
    /// <summary>
    /// Creates a timer that accepts a `timerCalc` function to perform
    /// calculated timeout retries, such as exponential backoff.
    /// </summary>
    public class Timer : IDisposable
    {
        private readonly Func<int, int> _timerCalc;
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private int _tries;

        public Timer(Action callback, Func<int, int> timerCalc)
        {
            _timerCalc = timerCalc;
            _timer.AutoReset = false;
            _timer.Elapsed += (sender, args) =>
            {
                _tries++;
                callback();
            };
        }

        public void Reset()
        {
            _tries = 0;
            _timer.Stop();
        }

        /// <summary>
        /// Cancels any previous ScheduleTimeout and schedules callback
        /// </summary>
        public void ScheduleTimeout()
        {
            _timer.Stop();
            _timer.Interval = _timerCalc(_tries + 1);
            _timer.Start();
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
