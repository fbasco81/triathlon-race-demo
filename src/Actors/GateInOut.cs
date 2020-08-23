using System;

namespace Actors
{
    public class GateInOut
    {
        public DateTime In { get; set; }
        public DateTime? Out { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}
