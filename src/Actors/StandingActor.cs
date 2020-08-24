using Akka.Actor;
using Messages;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;

namespace Actors
{

    public class LiveResult
    {
        public TimeSpan RacedTime { get; private set; }
        public int NrOfGates { get; private set; }
        public DateTime LastGatePassedAt { get; private set; }

        public LiveResult(TimeSpan racedTime, int nrOfGates, DateTime lastGatePassedAt)
        {
            RacedTime = racedTime;
            NrOfGates = nrOfGates;
            LastGatePassedAt = lastGatePassedAt;
        }
    }

    /// <summary>
    /// Actor that simulates traffic.
    /// </summary>
    public class StandingActor : UntypedActor
    {
        private List<Result> _results = new List<Result>();
        private List<AthleteDisqualified> _disqualifieds = new List<AthleteDisqualified>();
        private Dictionary<string,LiveResult> _liveStanding = new Dictionary<string, LiveResult>();
        private DateTime _raceStartedAt;
        private ICancelable _liveStandingScheduler;
        private int _entryRegisteredCounter;

        /// <summary>
        /// Handle received message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RaceStarted ss:
                    Handle(ss);
                    break;
                case AthleteRaceCompleted ss:
                    Handle(ss);
                    break;
                case PrintLiveStanding ss:
                    Handle(ss);
                    break;
                case AthleteDisqualified ad:
                    Handle(ad);
                    break;
                case AthleteEntryRegistered ad:
                    Handle(ad);
                    break;
                case AthleteCheckRegistered ad:
                    Handle(ad);
                    break;
                case RaceClosed rc:
                    Handle(rc);
                    break;
                case PrintFinalStanding ss:
                    Handle(ss);
                    break;
                case Shutdown sd:
                    Handle(sd);
                    break;
            }
        }

        private void Handle(RaceStarted msg)
        {
            _raceStartedAt = msg.Timestamp;
            
        }

        private void Handle(RaceClosed msg)
        {
            
        }

        private void Handle(AthleteEntryRegistered msg)
        {
            if (_entryRegisteredCounter == 0)
            {
                _liveStandingScheduler = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1),
                    Self,
                    new PrintLiveStanding(3),
                    Self);
            }
            _entryRegisteredCounter++;


            var racedTime = msg.Timestamp.Subtract(_raceStartedAt);
            if (_liveStanding.ContainsKey(msg.BibId))
            {
                var r = _liveStanding[msg.BibId];
                _liveStanding[msg.BibId] = new LiveResult(racedTime, r.NrOfGates+1, msg.Timestamp);
            }
            else
            {
                _liveStanding.Add(msg.BibId, new LiveResult(racedTime, 1, msg.Timestamp));
                //FluentConsole.White.Line($"Athlete {msg.BibId} has been added to the live standing");
            }
        }

        private void Handle(AthleteCheckRegistered msg)
        {
            var racedTime = msg.Timestamp.Subtract(_raceStartedAt);
            if (_liveStanding.ContainsKey(msg.BibId))
            {
                var r = _liveStanding[msg.BibId];
                _liveStanding[msg.BibId] = new LiveResult(racedTime, r.NrOfGates + 1, msg.Timestamp);
            }

            //FluentConsole.White.Line($"Athlete {msg.BibId} has been tracked at gate {msg.Gate} at {msg.Timestamp.ToString("HH:mm:ss.ffffff")} ");
        }

        /// <summary>
        /// Handle StartSimulation message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(AthleteRaceCompleted msg)
        {
            var result = new Result(msg.BibId, msg.Duration);
            if(_results.Count == 0)
            {
                _liveStandingScheduler.Cancel();
            }

            _results.Add(result);
        }

        private void Handle(AthleteDisqualified msg)
        {
            _disqualifieds.Add(msg);
            FluentConsole.Blue.Line($"Athlete {msg.BibId} has been disqualified {msg.GateAction} at gate {msg.Gate}");
        }

        private void Handle(PrintFinalStanding msg)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Overall standings at " + DateTime.Now.ToString("HH:mm:ss.ffffff"));
            foreach (var result in _results.OrderBy(x => x.Duration).Take(msg.Top))
            {
                sb.AppendLine($"Athlete {result.BibId} completed in {result.Duration.TotalMilliseconds} ms");
            }
            FluentConsole.DarkGreen.Line(sb.ToString());
        }

        private void Handle(Shutdown msg)
        {
            Context.Stop(Self);
        }

        private void Handle(PrintLiveStanding msg)
        {
            if (_liveStanding.Count() == 0) return;

            var sb = new StringBuilder();
            sb.AppendLine("Live standings at " + DateTime.Now.ToString("HH:mm:ss.ffffff"));
            var maxGate = _liveStanding.Max(x => x.Value.NrOfGates);
            var partialResult = new List<string>();
            while(maxGate > 0 && partialResult.Count() < msg.Top)
            {
                foreach (var result in _liveStanding.Where(x => x.Value.NrOfGates == maxGate)
                                            .OrderBy(x => x.Value.RacedTime)
                                            .Take(msg.Top))
                {
                    partialResult.Add($"Athlete {result.Key} is winning " +
                        $"in {result.Value.RacedTime.TotalMilliseconds} ms " +
                        $"after {result.Value.NrOfGates} gate(s) " +
                        $"passed at {result.Value.LastGatePassedAt.ToString("HH:mm:ss.ffffff")}");
                }

                maxGate--;
            }

            foreach (var text in partialResult.Take(msg.Top))
            {
                sb.AppendLine(text);
            }
            FluentConsole.DarkGreen.Line(sb.ToString());
        }


    }
}
