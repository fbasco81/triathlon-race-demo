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
    public class PhoneNotificationActor : UntypedActor, IWithUnboundedStash
    {
        public IStash Stash { get; set; }
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

        //[System.Diagnostics.DebuggerHidden]
        //public override void AroundPreRestart(Exception cause, object message)
        //{

        //}

        //public override void AroundPostRestart(Exception cause, object message)
        //{
        //    if (cause is TimeoutException)
        //    {
        //        if (message is SendPhoneNotification sendResult)
        //        {
        //            //Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self,
        //            //    new SendPhoneNotification(sendResult.BibId, sendResult.Duration, sendResult.Position),
        //            //    Sender);
        //            Self.Tell(message);

        //        }
        //    }
        //    base.AroundPostRestart(cause, message);
        //}

        protected override void PostRestart(Exception reason)
        {
            if (reason is TimeoutException)
            {
                Stash.UnstashAll();
            }
            base.PostRestart(reason);
        }

        protected override void PreRestart(Exception reason, object message)
        {
            if (reason is TimeoutException)
            {
                Stash.Stash();
                //if (message is SendPhoneNotification sendResult)
                //{
                //    //Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), Self,
                //    //    new SendPhoneNotification(sendResult.BibId, sendResult.Duration, sendResult.Position),
                //    //    Sender);
                //    Self.Tell(message);

                //}
            }
            base.PreRestart(reason, message);
        }

        [System.Diagnostics.DebuggerHidden]
        private void Handle(SendPhoneNotification msg)
        {
            var rnd = new Random();
            var n = rnd.Next(1, 8);
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
