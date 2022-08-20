using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yort.Ntp;

namespace DGScope
{
    public class TimeSync
    {
        private static TimeSpan offset = TimeSpan.Zero;
        private static Stopwatch stopwatch = new Stopwatch();
        public bool Synchronized { get; private set; } = false;
        public string Server { get; set; } = "pool.ntp.org";
        public TimeSpan TimeSyncInterval { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSync()
        {
            Resync();
        }
        public DateTime CurrentTime()
        {
            if (stopwatch.Elapsed >= TimeSyncInterval)
                Resync();
            return DateTime.UtcNow + offset;
        }
        private async Task Resync()
        {
            stopwatch.Restart();
            var client = new NtpClient(Server);
            var delay = TimeSpan.FromSeconds(1);
            while (true)
            {
                try
                {
                    var currentTime = await client.RequestTimeAsync();
                    offset = currentTime.NtpTime - DateTime.UtcNow;
                    Synchronized = true;
                    break;
                }
                catch
                {
                    Synchronized = false;
                    Thread.Sleep(delay);
                    delay += delay;
                }
            }

        }
    }
}
