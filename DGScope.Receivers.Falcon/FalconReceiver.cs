﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DGScope.Receivers.Falcon
{
    public class FalconReceiver : Receiver
    {
        private PlaybackControlForm PlaybackForm;
        private FalconFile file = null;
        private Dictionary<int, Aircraft> trackDictionary = new Dictionary<int, Aircraft>();

        private DateTime lastUpdate;
        private TimeSpan manualAdjust = TimeSpan.Zero;
        private Stopwatch stopwatch = new Stopwatch();
        private Timer timer;

        internal DateTime CurrentTime
        {
            get => lastUpdate + stopwatch.Elapsed + manualAdjust;
            set
            {
                manualAdjust = value - CurrentTime;
            }
        }

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
        internal FalconFile File
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
                    file.Updates.Sort((x, y) => DateTime.Compare(x.Time, y.Time));
                    stopwatch.Reset();
                    lastUpdate = StartOfData.Value;
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
                timer.Change(10, 10);
            }
            stopwatch.Start();
            Playing = true;
        }
        internal void Pause()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            stopwatch.Stop();
            Playing = false;
        }

        private void timerCallback(object state)
        {
            stopwatch.Stop();
            RadarWindow.CurrentTime = CurrentTime;
            var updates = file.Updates.Where(x => x.Time > lastUpdate && x.Time <= CurrentTime);
            updates.ToList().ForEach(x => sendUpdate(x));
            lastUpdate = CurrentTime;
            manualAdjust = TimeSpan.Zero;
            stopwatch.Restart();
            PlaybackForm.UpdateCallback();
        }

        private void sendUpdate(FalconUpdate update)
        {
            var plane = GetPlane(update.TrackID);
            plane.FlightPlanCallsign = update.ACID;
            plane.Squawk = update.ReportedBeaconCode;
            if (update.Altitude != null)
            {
                plane.Altitude = update.Altitude;
            }
            else
            {
                plane.Altitude = new Altitude() { AltitudeType = AltitudeType.Unknown };
            }
            if (update.Track.HasValue)
            {
                plane.SetTrack(update.Track.Value, update.Time);
            }
            if (update.Speed.HasValue)
            {
                plane.GroundSpeed = update.Speed.Value;
            }
            if (plane.PositionInd != update.Owner)
            {
                plane.PositionInd = update.Owner;
            }
            plane.FlightRules = update.FlightRules;
            plane.Category = update.Category;
            plane.Scratchpad = update.Scratchpad1;
            plane.Scratchpad2 = update.Scratchpad2;
            plane.Type = update.Type;
            plane.PendingHandoff = update.PendingHandoff;
            plane.SetLocation(update.Location, update.Time);
            plane.LastMessageTime = update.Time;
            if (update.ModeSAddress.HasValue)
            {
                plane.ModeSCode = update.ModeSAddress.Value;
            }
            plane.Callsign = update.BcastFLID;
        }
        public override void Start()
        {
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

        private Aircraft GetPlane(int trackID)
        {
            Aircraft plane;
            lock (trackDictionary)
            {
                if (!trackDictionary.TryGetValue(trackID, out plane))
                {
                    plane = GetNewPlane(trackID);
                }
            }
            return plane;
        }
        private Aircraft GetNewPlane(int trackID)
        {
            Aircraft plane = GetPlane(Guid.NewGuid(), true);
            trackDictionary.Add(trackID, plane);
            return plane;
        }
    }
}
