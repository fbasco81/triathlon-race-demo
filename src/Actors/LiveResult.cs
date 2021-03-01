using System;

namespace Actors
{
    public class LiveResult
    {
        public TimeSpan RacedTime { get; private set; }
        public int NrOfGates { get; private set; }
        public DateTime LastGatePassedAt { get; private set; }

        public LiveResult(TimeSpan racedTime, int nrOfGates, DateTime lastGatePassedAt)
        {
            RacedTime = racedTime;
            NrOfGates = nrOfGates;
            LastGatePassedAt = lastGatePassedAt;
        }
    }
}
