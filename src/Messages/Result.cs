using System;

namespace Messages
{
    public class Result
    {
        public string BibId { get; private set; }

        public TimeSpan Duration { get; private set; }

        public Result(string bibId, TimeSpan duration)
        {
            BibId = bibId;
            Duration = duration;

        }
    }
}
