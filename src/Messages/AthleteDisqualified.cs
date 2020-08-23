using System;

namespace Messages
{
    public class AthleteDisqualified
    {
        public string BibId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public Gates Gate { get; private set; }

        public GateActions GateAction { get; private set; }

        public AthleteDisqualified(string bibId, DateTime timestamp, Gates gate, GateActions gateAction)
        {
            BibId = bibId;
            Timestamp = timestamp;
            Gate = gate;
            GateAction = gateAction;
        }
    }
}
