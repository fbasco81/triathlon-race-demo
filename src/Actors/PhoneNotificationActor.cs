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
    public class PhoneNotificationActor : UntypedActor
    {
        public PhoneNotificationActor()
        {
     
        }

        /// <summary>
        /// Handle received message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        [System.Diagnostics.DebuggerHidden]
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case SendPhoneNotification ss:
                    Handle(ss);
                    break;
                case Shutdown sd:
                    Handle(sd);
                    break;
            }
        }

        protected override void PreRestart(Exception reason, object message)
        {
            // put message back in mailbox for re-processing after restart
            Self.Tell(message);
        }

        [System.Diagnostics.DebuggerHidden]
        private void Handle(SendPhoneNotification msg)
        {
            FluentConsole.Yellow.Line($"[{Self.Path.Uid}]: Congratulation athlete {msg.BibId}. You have ranked {msg.Position} with a duration of {msg.Duration}");
        }
        
        private void Handle(Shutdown msg)
        {

            Context.Stop(Self);
        }
    }
}
