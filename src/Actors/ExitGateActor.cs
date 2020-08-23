using Akka.Actor;
using Messages;
using System;

namespace Actors
{
    /// <summary>
    /// Actor that represents an exit camera.
    /// </summary>
    public class ExitGateActor : UntypedActor
    {
        private ActorSelection _raceControlActor;

        public ExitGateActor()
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
        /// Handle VehiclePassed message
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(AthletePassed msg)
        {
            var athleteExitRegistered = new AthleteExitRegistered(msg.BibId, msg.Timestamp, msg.Gate);
            _raceControlActor.Tell(athleteExitRegistered);
            //Console.WriteLine("Athlete {0} exited gate", msg.BibId);
        }
    }
}
