using Akka.Actor;
using Messages;
using System;

namespace Actors
{
    /// <summary>
    /// Actor that represents an intermediate gate.
    /// </summary>
    public class IntermediateGateActor : UntypedActor
    {
        private ActorSelection _raceControlActor;

        public IntermediateGateActor()
        {
            // initialize state
            _raceControlActor = Context.System.ActorSelection("/user/race-control");
        }

        /// <summary>
        /// Handle received message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case AthletePassed vp:
                    Handle(vp);
                    break;
            }
        }

        /// <summary>
        /// Handle AthletePassed message
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(AthletePassed msg)
        {
            var athleteCheckRegistered = new AthleteCheckRegistered(msg.BibId, msg.Timestamp, msg.Gate);
            _raceControlActor.Tell(athleteCheckRegistered);
        }
    }
}
