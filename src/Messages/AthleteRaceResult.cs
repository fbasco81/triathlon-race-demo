using System;
using System.Collections.Generic;
using System.Text;

namespace Messages
{
    public class AthleteRaceResult
    {
        public string BibId { get; private set; }
        public Dictionary<string, GateInOut> Gates { get; private set; }
        public AthleteRaceResult(string bibId, Dictionary<string, GateInOut> gates)
        {
            BibId = bibId;
            Gates = gates;
        }
    }
}
