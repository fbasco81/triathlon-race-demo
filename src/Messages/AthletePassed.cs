using System;

namespace Messages
{
    public class AthletePassed
    {
        public string BibId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public AthletePassed(string bibId, DateTime timestamp)
        {
            BibId = bibId;
            Timestamp = timestamp;
        }
    }
}
