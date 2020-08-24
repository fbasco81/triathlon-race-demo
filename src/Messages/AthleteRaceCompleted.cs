using System;
using System.Collections.Generic;
using System.Text;

namespace Messages
{
    public class AthleteRaceCompleted
    {
        public string BibId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public TimeSpan Duration { get; private set; }

        public AthleteRaceCompleted(string bibId, DateTime timestamp, TimeSpan duration)
        {
            BibId = bibId;
            Timestamp = timestamp;
            Duration = duration;
        }
    }

    public class SwimCompleted
    {
        public string BibId { get; private set; }

        public TimeSpan Duration { get; private set; }

        public SwimCompleted(string bibId, TimeSpan duration)
        {
            BibId = bibId;
            Duration = duration;
        }
    }

    public class BikeCompleted
    {
        public string BibId { get; private set; }

        public TimeSpan Duration { get; private set; }

        public BikeCompleted(string bibId, TimeSpan duration)
        {
            BibId = bibId;
            Duration = duration;
        }
    }

    public class RunCompleted
    {
        public string BibId { get; private set; }

        public TimeSpan Duration { get; private set; }

        public RunCompleted(string bibId, TimeSpan duration)
        {
            BibId = bibId;
            Duration = duration;
        }
    }
}
