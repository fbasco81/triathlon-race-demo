using Akka.Actor;
using Messages;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;

namespace Actors
{

    
    /// <summary>
    /// Actor that simulates standing.
    /// </summary>
    public class NotificationActor : UntypedActor
    {
        
        public NotificationActor()
        {
        }

        /// <summary>
        /// Handle received message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case SendPersonalResult ss:
                    Handle(ss);
                    break;
                case Shutdown sd:
                    Handle(sd);
                    break;
            }
        }

        protected override void PreRestart(Exception reason, object message)
        {
            if (reason is TimeoutException)
            {
                Self.Tell(message);
            }
            base.PreRestart(reason, message);
        }

        private void Handle(SendPersonalResult msg)
        {
            var rnd = new Random();
            var n = rnd.Next(1, 5);
            if (n == 2)
            {
                FluentConsole.Red.Line($"[{Self.Path.Uid}]: Athlete {msg.BibId} is not available now");
                throw new TimeoutException($"Athlete {msg.BibId} has his phone unreachable");
            }
            FluentConsole.Yellow.Line($"[{Self.Path.Uid}]: Congratulation athlete {msg.BibId}. You have ranked {msg.Position} with a duration of {msg.Duration}");
        }

        private void Handle(Shutdown msg)
        {
            
            Context.Stop(Self);
        }
    }
}
