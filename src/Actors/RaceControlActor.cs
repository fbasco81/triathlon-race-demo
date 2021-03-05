using Akka.Actor;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Actors
{
    /// <summary>
    /// Actor that handles race control.
    /// </summary>
    public class RaceControlActor : UntypedActor
    {
        ActorSelection _standingActor;
        public RaceControlActor()
        {
            _standingActor = Context.System.ActorSelection($"/user/standing");
        }

        /// <summary>
        /// Handle received message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case AthleteRegistered ver:
                    Handle(ver);
                    break;
                case AthleteEntryRegistered ver:
                    Handle(ver);
                    break;
                case AthleteCheckRegistered ver:
                    Handle(ver);
                    break;
                case AthleteExitRegistered vxr:
                    Handle(vxr);
                    break;
                case RaceStarted ver:
                    Handle(ver);
                    break;
                case RaceClosed ver:
                    Handle(ver);
                    break;

            }
        }

        private void Handle(AthleteRegistered msg)
        {
            var props = Props.Create<AthleteSwitchableActor>(msg.BibId);
            var athleteActor = Context.ActorOf(props, $"athlete-{msg.BibId}");
        }

        /// <summary>
        /// Handle AthleteEntryRegistered message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(AthleteEntryRegistered msg)
        {
            var athleteActor = Context.ActorSelection($"/user/race-control/*/athlete-{msg.BibId}");

            athleteActor.Tell(msg, Self);
        }

        /// <summary>
        /// Handle VehicleExitRegistered message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(AthleteExitRegistered msg)
        {
            var athleteActor = Context.ActorSelection($"/user/race-control/*/athlete-{msg.BibId}");
            athleteActor.Tell(msg);
        }

        private void Handle(AthleteCheckRegistered msg)
        {
            var standingActor = Context.ActorSelection($"/user/standing");
            standingActor.Tell(msg);
        }

        private void Handle(RaceStarted msg)
        {
            _standingActor.Tell(msg);
        }

        private void Handle(RaceClosed msg)
        {
            var athleteActor = Context.ActorSelection($"/user/race-control/*/*");
            athleteActor.Tell(new RaceClosed());

            _standingActor.Tell(new PrintFinalStanding(10));
        }
    }
}
