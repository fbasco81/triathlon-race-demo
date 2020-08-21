using System;
using System.Collections.Generic;
using System.Text;

namespace Messages
{
    public class SimulatePassingAthlete
    {
        public string BibId { get; private set; }
        public DateTime RaceStartedAt { get; private set; }

        public SimulatePassingAthlete(string bibId, DateTime raceStartedAt)
        {
            BibId = bibId;
            raceStartedAt = RaceStartedAt;
        }
    }
}
