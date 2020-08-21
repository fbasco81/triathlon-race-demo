using System;

namespace Messages
{
    public class RaceStarted
    {
        public string BibId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public RaceStarted(string bibId, DateTime timestamp)
        {
            BibId = bibId;
            Timestamp = timestamp;
        }
    }
}
