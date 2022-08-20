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
        private bool initialsync = false;
        public bool Synchronized { get; private set; } = false;
        public string Server { get; set; } = "pool.ntp.org";
        public TimeSpan TimeSyncInterval { get; set; } = TimeSpan.FromMinutes(10);
        public DateTime CurrentTime()
        {
            if (initialsync && stopwatch.Elapsed < TimeSyncInterval)
                return DateTime.UtcNow + offset;
            Task.Run(Resync);
            return DateTime.UtcNow + offset;
        }
        public async Task Resync()
        {
            Synchronized = false;
            initialsync = true;
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
                    Thread.Sleep(delay);
                    delay += delay;
                }
            }

        }
    }
}
