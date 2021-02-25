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

       

        private void Handle(SendPersonalResult msg)
        {
            FluentConsole.Yellow.Line($"Congratulation athlete {msg.BibId}. You have ranked {msg.Position} with a duration of {msg.Duration}");
        }

        private void Handle(Shutdown msg)
        {
            
            Context.Stop(Self);
        }
    }
}
