using System;
using System.Collections.Generic;

namespace Messages
{
    public class SendNotification
    {
        public List<Result> Results { get; private set; }
        
        public SendNotification(List<Result> results)
        {
            Results = results;
        }
    }
        
}
