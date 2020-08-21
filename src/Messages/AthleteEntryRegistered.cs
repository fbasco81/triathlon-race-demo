using System;

namespace Messages
{
    public class AthleteEntryRegistered
    {
        public string BibId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public AthleteEntryRegistered(string bibId, DateTime timestamp)
        {
            BibId = bibId;
            Timestamp = timestamp;
        }
    }
}
