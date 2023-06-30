using Newtonsoft.Json;
using System.ComponentModel;
using System;
using System.IO;
using System.Threading;

namespace DGScope.STARS
{
    [Serializable()]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [JsonObject]
    public class ClockPhase
    {
        private static Timer timer;
        private static int phase;
        private static int phasecount;
        public Sequence PhaseSequence { get; set; }
        public double Interval1 { get; set; } = 2.0;
        public double Interval2 { get; set; } = 1.5;
        public double Interval3 { get; set; } = 2.0;
        public double Interval4 { get; set; } = 1.5;
        public double Interval5 { get; set; } = 0.0;
        public double Interval6 { get; set; } = 0.0;
        [Browsable(false)]
        public int Phase 
        { 
            get
            {
                if (timer == null)
                {
                    timer = new Timer(new TimerCallback(cbPhaseAdvance), null, 0, (int)(Interval1 * 1000));
                    phase = 0;
                    phasecount = 0;
                }
                return phase;
            } 
        }
        private void cbPhaseAdvance(object state)
        {
            switch (PhaseSequence)
            {
                case Sequence.ONE_TWO_ONE_THREE when phasecount == 0:
                    phase = 1;
                    phasecount = 1;
                    timer.Change((int)(Interval2 * 1000), (int)(Interval2 * 1000));
                    break;
                case Sequence.ONE_TWO_ONE_THREE when phasecount == 1:
                    phase = 0;
                    phasecount = 2;
                    timer.Change((int)(Interval3 * 1000), (int)(Interval3 * 1000));
                    break;
                case Sequence.ONE_TWO_ONE_THREE when phasecount == 2:
                    phase = 2;
                    phasecount = 3;
                    timer.Change((int)(Interval4 * 1000), (int)(Interval4 * 1000));
                    break;
                case Sequence.ONE_TWO_ONE_THREE when phasecount == 3:
                    phase = 0;
                    phasecount = 0;
                    timer.Change((int)(Interval1 * 1000), (int)(Interval1 * 1000));
                    break;
            }
        }
    }
    public enum Sequence
    {
        ONE_TWO_ONE_THREE
    }
}
