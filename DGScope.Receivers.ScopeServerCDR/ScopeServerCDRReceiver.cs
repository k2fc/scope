using DGScope.Library;
using DGScope.Receivers.ScopeServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DGScope.Receivers.Falcon
{
    public class ScopeServerCDRReceiver : Receiver
    {
        private PlaybackControlForm PlaybackForm;
        private CDRFile file = null;
        private ScopeServerClient client = new ScopeServerClient()
        {
            Enabled = false
        };

        private DateTime lastUpdate;
        private TimeSpan manualAdjust = TimeSpan.Zero;
        private Stopwatch stopwatch = new Stopwatch();
        private Timer timer;
        private double speed = 1.0;
        private int timerInverval => (int)(250 / speed);

        public bool IncludeUncorrelated { get; set; } = false;
        public ScopeServerCDRReceiver() : base()
        {
            timer = new Timer(timerCallback, null, timerInverval, timerInverval);
        }
        internal double Speed
        {
            get=> speed;
            set
            {
                speed = value;
                if (timer != null)
                {
                    timer.Change(timerInverval, timerInverval);
                }
            }
        }
        internal DateTime CurrentTime
        {
            get => lastUpdate + TimeSpan.FromMilliseconds(stopwatch.Elapsed.TotalMilliseconds * Speed) + manualAdjust;
            set
            {
                lock (working)
                {
                    lock (aircraft)
                    {
                        aircraft.ToList().ForEach(x => aircraft.Remove(x));
                    }
                    client.Tracks.Clear();
                    client.FlightPlans.Clear();
                    var updates = file.Updates.Where(x => x.TimeStamp <= value).ToList();
                    client.WeatherRadars.Clear();
                    SendUpdates(updates);
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
                    CurrentTime = StartOfData.Value;
                    lastUpdate = StartOfData.Value;
                    Sites = file.Sites;
                    aircraft.Clear();
                    client.FlightPlans.Clear();
                    client.Tracks.Clear();
                    if (file.Updates.Count > 0)
                    {
                        SendUpdate(file.Updates.First());
                    }
                }
            }
        }
        internal void AddFile(CDRFile file)
        {
            if (File == null)
            {
                File = file;
            }
            else
            {
                File.Updates.AddRange(file.Updates);
                File.Updates.Sort((x, y) => DateTime.Compare(x.TimeStamp, y.TimeStamp));
                stopwatch.Reset();
                lastUpdate = StartOfData.Value;
                Sites = file.Sites;
                aircraft.Clear();
                client.FlightPlans.Clear();
                client.Tracks.Clear();
            }
        }
        internal void Play()
        {
            if (timer == null)
            {
                timer = new Timer(timerCallback, null, timerInverval, timerInverval);
            }
            else
            {
                timer.Change(timerInverval, timerInverval);
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
            //lock (working)  
            //{
            var oldtime = lastUpdate;
            var newtime = CurrentTime;
            if (newtime > DateTime.MinValue)
            {
                RadarWindow.CurrentTime = newtime;
            }
            if (Playing)
            {
                lastUpdate = newtime;
                manualAdjust = TimeSpan.Zero;
                stopwatch.Restart();
                var updates = file.Updates.Where(x => x.TimeStamp > oldtime && x.TimeStamp <= newtime).ToList();
                updates.ForEach(x => SendUpdate(x));
                PlaybackForm.UpdateCallback();
            }
            //}
        }

        private void SendUpdates(IEnumerable<Update> Updates)
        {
            foreach (var update in Updates)
            {
                Task.Run(() => SendUpdate(update));
            }
        }

        private void SendUpdate(Update update)
        {
            _ = client.ProcessUpdate(update);
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
