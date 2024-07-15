using DGScope.Library;
using DGScope.Receivers.ScopeServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DGScope.Receivers.Falcon
{
    public class ScopeServerCDRReceiver : Receiver
    {
        private PlaybackControlForm PlaybackForm;
        private CDRFile file = null;
        private Dictionary<int, Aircraft> trackDictionary = new Dictionary<int, Aircraft>();
        private ScopeServerClient client = new ScopeServerClient()
        {
            Enabled = false
        };

        private DateTime lastUpdate;
        private TimeSpan manualAdjust = TimeSpan.Zero;
        private Stopwatch stopwatch = new Stopwatch();
        private Timer timer;

        public bool IncludeUncorrelated { get; set; } = false;
        internal double Speed { get; set; } = 1.0d;
        internal DateTime CurrentTime
        {
            get => lastUpdate + TimeSpan.FromMilliseconds(stopwatch.Elapsed.TotalMilliseconds * Speed) + manualAdjust;
            set
            {
                lock (working)
                {
                    manualAdjust = value - CurrentTime;
                    lock (aircraft)
                    {
                        lock (trackDictionary)
                        {
                            aircraft.ToList().ForEach(x => aircraft.Remove(x));
                            //aircraft.Clear();
                            trackDictionary.Clear();
                        }
                    }
                    var updates = file.Updates.Where(x => x.TimeStamp <= value).ToList();
                    updates.ForEach(x => sendUpdate(x));
                    PlaybackForm.UpdateCallback();
                }
            }
        }

        internal List<string> Sites { get; set; } = new List<string>();
        internal string SelectedSite { get; set; } = string.Empty;

        internal bool Playing = false;
        internal DateTime? StartOfData
        {
            get
            {
                if (File == null)
                {
                    return null;
                }
                return File.StartOfData;
            }
        }
        internal DateTime? EndOfData
        {
            get
            {
                if (File == null)
                {
                    return null;
                }
                return File.EndOfData;
            }
        }
        internal TimeSpan? LengthOFData
        {
            get
            {
                if (File == null)
                {
                    return null;
                }
                return File.LengthOfData;
            }
        }
        internal CDRFile File
        {
            get
            {
                return file;
            }
            set
            {
                file = value;
                if (file != null)
                {
                    //file.Updates.Sort((x, y) => DateTime.Compare(x.TimeStamp, y.TimeStamp));
                    stopwatch.Reset();
                    lastUpdate = StartOfData.Value;
                    Sites = file.Sites;
                    aircraft.Clear();
                    client.FlightPlans.Clear();
                    client.Tracks.Clear();
                }
            }
        }

        internal void Play()
        {
            if (timer == null)
            {
                timer = new Timer(timerCallback, null, 100, 100);
            }
            else
            {
                timer.Change(100, 100);
            }
            stopwatch.Start();
            Playing = true;
        }
        internal void Pause()
        {
            //timer.Change(Timeout.Infinite, Timeout.Infinite);
            stopwatch.Stop();
            Playing = false;
        }
        object working = new object();
        private void timerCallback(object state)
        {
            lock (working)  
            {
                var oldtime = lastUpdate;
                var newtime = CurrentTime;
                RadarWindow.CurrentTime = newtime;
                if (Playing)
                {
                    lastUpdate = newtime;
                    manualAdjust = TimeSpan.Zero;
                    stopwatch.Restart();
                    var updates = file.Updates.Where(x => x.TimeStamp > oldtime && x.TimeStamp <= newtime).ToList();
                    updates.ForEach(x => sendUpdate(x));
                    PlaybackForm.UpdateCallback();
                }
            }
        }

        private void sendUpdate(Update update)
        {
            lock (client)
            {
              _ = client.ProcessUpdate(update);
            }
        }

        public override void SetWeatherRadarDisplay(NexradDisplay weatherRadar)
        {
            client.SetWeatherRadarDisplay(weatherRadar);
        }
        public override void Start()
        {
            client.SetAircraftList(this.aircraft);
            if (PlaybackForm == null)
            {
                PlaybackForm = new PlaybackControlForm(this);
            }
            if (!PlaybackForm.Visible)
            {
                PlaybackForm.Show();
            }
        }

        public override void Stop()
        {
            if (PlaybackForm != null)
            {
                PlaybackForm.Hide();
            }
        }
    }
}
