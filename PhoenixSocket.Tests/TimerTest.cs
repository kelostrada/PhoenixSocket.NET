using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PhoenixSocket.Tests
{
    [TestClass]
    public class TimerTest
    {
        [TestMethod]
        public void TestTimer()
        {
            Func<int, int> reconnectAfterMs = tries => 
                tries > 2 ? 1000 : new[] { 100, 500 }[tries - 1];
            var timer = new Timer(() => Trace.WriteLine("A"), reconnectAfterMs);
            timer.ScheduleTimeout();
            Thread.Sleep(200);
            Trace.WriteLine("Slept 200");
            timer.ScheduleTimeout();
            Thread.Sleep(600);
            Trace.WriteLine("Slept 600");
            timer.ScheduleTimeout();
            Thread.Sleep(1100);
            Trace.WriteLine("Slept 1100");

        }
    }
}
