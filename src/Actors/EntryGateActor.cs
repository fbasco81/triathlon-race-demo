using Akka.Actor;
using Messages;
using System;

namespace Actors
{
    /// <summary>
    /// Actor that represents an entry camera.
    /// </summary>
    public class EntryGateActor : UntypedActor
    {
        private ActorSelection _raceControlActor;

        public EntryGateActor()
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
            var athleteEntryRegistered = new AthleteEntryRegistered(msg.BibId, msg.Timestamp, msg.Gate);
            _raceControlActor.Tell(athleteEntryRegistered);
            //Console.WriteLine("Athlete {0} entered gate", msg.BibId);
        }
    }
}
