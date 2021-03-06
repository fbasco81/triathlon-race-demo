using Akka.Actor;
using Messages;
using System;
using System.Collections.Generic;

namespace Actors
{
    public class ExitDelayInSecond
    {
        public int Min { get; set; }

        public int Max { get; set; }
    }

    public class GateInfo
    {
        public ExitDelayInSecond ExitDelay { get; set; }

        public int NrOfIntermediateChecks { get; set; }
    }


    /// <summary>
    /// Actor that simulates the race.
    /// </summary>
    public class SimulationActor : UntypedActor
    {
        private int _numberOfAthletes;
        private int _athletesSimulated;
        private string _randomWinner;
        private string _randomDisqualified;
        private Random _rnd;
        private TimeSpan _raceDuration = TimeSpan.FromSeconds(12);

        private int _minTransitionDelayInS = 1;
        private Dictionary<Gates, GateInfo> _exitDelay;

        public SimulationActor(Dictionary<Gates, GateInfo> gates)
        {
            _exitDelay = gates;
        }

        /// <summary>
        /// Handle received message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case StartSimulation ss:
                    Handle(ss);
                    break;
                case SimulatePassingAthlete spc:
                    Handle(spc);
                    break;
                case Shutdown sd:
                    Context.Stop(Self);
                    break;
            }
        }
        
        /// <summary>
        /// Handle StartSimulation message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(StartSimulation msg)
        {
            // initialize state
            _numberOfAthletes = msg.NumberOfAthletes;
            _athletesSimulated = 0;
            _rnd = new Random();

            var raceStartedAt = DateTime.Now;
            _randomWinner = _rnd.Next(1, msg.NumberOfAthletes).ToString();
            _randomDisqualified = _randomWinner;
            while (_randomDisqualified == _randomWinner)
            {
                _randomDisqualified = _rnd.Next(1, msg.NumberOfAthletes).ToString();
            }

            FluentConsole.Magenta.Line($"Winner should be #{_randomWinner}");
            FluentConsole.Magenta.Line($"Disqualified should be #{_randomDisqualified}");

            var raceControl = Context.System.ActorSelection($"/user/race-control");
            for (int i = 0; i < msg.NumberOfAthletes; i++)
            {
                var bibId = (i + 1).ToString();
                raceControl.Tell(new AthleteRegistered(bibId));
            }
                        
            raceControl.Tell(new RaceStarted(raceStartedAt));

            for (int i = 0; i < msg.NumberOfAthletes; i++)
            {
                var bibId = (i + 1).ToString();
                Self.Tell(new SimulatePassingAthlete(bibId, raceStartedAt));
            }

            Context.System.Scheduler.ScheduleTellOnce(_raceDuration,
                raceControl,
                new RaceClosed(),
                Self);


        }

        private void Handle(SimulatePassingAthlete msg)
        {
            _athletesSimulated++;
            var isWinner = msg.BibId == _randomWinner;

            DateTime entryTimestamp = DateTime.Now;
          
            var counter = 1;
            foreach (var kv in _exitDelay)
            {
                var athletePassedAsEntered = new AthletePassed(msg.BibId, entryTimestamp, kv.Key);
                ActorSelection entryGate = Context.System.ActorSelection($"/user/entrygate{kv.Key.ToString().ToLower()}");
                Context.System.Scheduler.ScheduleTellOnce(
                    computeMessageDelay(entryTimestamp), //delay
                    entryGate, 
                    athletePassedAsEntered, 
                    Self);
          
                var gateDelay = !isWinner ?
                    TimeSpan.FromSeconds(_rnd.Next(kv.Value.ExitDelay.Min, kv.Value.ExitDelay.Max) + _rnd.NextDouble())
                    : TimeSpan.FromSeconds(kv.Value.ExitDelay.Min);
                
                for (int i = 0; i < kv.Value.NrOfIntermediateChecks; i++)
                {
                    ActorSelection gate = Context.System.ActorSelection($"/user/intermediategate-{kv.Key.ToString().ToLower()}-{i+1}");
                    var intermediateFractionDelay = gateDelay.TotalMilliseconds / (kv.Value.NrOfIntermediateChecks + 1);
                    var intermediateDelay = TimeSpan.FromMilliseconds(intermediateFractionDelay * (i+1));
                    var intermediateTimestamp = entryTimestamp.Add(intermediateDelay);
                    var athletePasedIntermediateCheck = new AthletePassed(msg.BibId, intermediateTimestamp, kv.Key);
                    Context.System.Scheduler.ScheduleTellOnce(
                        computeMessageDelay(intermediateTimestamp),
                        gate,
                        athletePasedIntermediateCheck, 
                        Self);
                }

                DateTime exitTimestamp = entryTimestamp.Add(gateDelay);
                var athletePassedAsExited = new AthletePassed(msg.BibId, exitTimestamp, kv.Key);
                ActorSelection exityGate = Context.System.ActorSelection($"/user/exitgate{kv.Key.ToString().ToLower()}");
                if (msg.BibId == _randomDisqualified && kv.Key == Gates.Run)
                {
                    FluentConsole.Magenta.Line($"Athlete #{_randomDisqualified} should be disqualified for missing exiting gate {kv.Key}");
                }
                else
                {
                    Context.System.Scheduler.ScheduleTellOnce(
                        computeMessageDelay(exitTimestamp), //delay, 
                        exityGate, 
                        athletePassedAsExited, 
                        Self);
                }

                if (counter < _exitDelay.Count)
                {
                    var transitionTime = !isWinner ?
                        TimeSpan.FromSeconds(_rnd.Next(_minTransitionDelayInS, _minTransitionDelayInS) + _rnd.NextDouble())
                        : TimeSpan.FromSeconds(_minTransitionDelayInS);
                    entryTimestamp = exitTimestamp.Add(transitionTime);
                }

                counter++;
            }

            if (_athletesSimulated == _numberOfAthletes)
            {
                Self.Tell(new Shutdown());
            }
        }

        private TimeSpan computeMessageDelay(DateTime timespan)
        {
            var delay = timespan.Subtract(DateTime.Now);
            return delay.TotalMilliseconds > 0? delay : TimeSpan.FromMilliseconds(0);
        }
    }
}
