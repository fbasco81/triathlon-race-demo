using System;

namespace Messages
{
    public class SendPersonalResult
    {
        public string BibId { get; private set; }

        public TimeSpan Duration{ get; private set; }

        public int Position { get; private set; }

        public SendPersonalResult(string bibId, TimeSpan duration, int position)
        {
            BibId = bibId;
            Duration = duration;
            Position = position;
        }
    }
        
}
