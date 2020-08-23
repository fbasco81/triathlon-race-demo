using System;

namespace Messages
{
    public class RaceStarted
    {
        public DateTime Timestamp { get; private set; }

        public RaceStarted(DateTime timestamp)
        {
            Timestamp = timestamp;
        }
    }
}
