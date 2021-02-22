﻿using Akka.Actor;
using Messages;
using System.Collections.Generic;
using System.Linq;

namespace Actors
{
    /// <summary>
    /// Actor that simulates bike standing .
    /// </summary>
    public class BikeStandingActor : UntypedActor
    {
        private List<Result> _results = new List<Result>();

        /// <summary>
        /// Handle received message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case BikeCompleted ss:
                    Handle(ss);
                    break;
                case Shutdown sd:
                    Handle(sd);
                    break;
            }
        }

        /// <summary>
        /// Handle BikeCompleted message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(BikeCompleted msg)
        {
            var result = new Result(msg.BibId, msg.Duration);

            _results.Add(result);
        }

        private void Handle(Shutdown msg)
        {
            Context.Stop(Self);
        }
    }
}
