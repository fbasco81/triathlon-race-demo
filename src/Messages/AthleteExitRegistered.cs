using System;

namespace Messages
{
    public class AthleteExitRegistered
    {
        public string BibId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public AthleteExitRegistered(string bibId, DateTime timestamp)
        {
            BibId = bibId;
            Timestamp = timestamp;
        }
    }
}
