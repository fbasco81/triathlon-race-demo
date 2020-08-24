using System;

namespace Messages
{
    public class AthleteCheckRegistered
    {
        public string BibId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public Gates Gate { get; private set; }

        public AthleteCheckRegistered(string bibId, DateTime timestamp, Gates gate)
        {
            BibId = bibId;
            Timestamp = timestamp;
            Gate = gate;
        }
    }

    


}
