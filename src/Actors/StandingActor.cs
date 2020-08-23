using Akka.Actor;
using Messages;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;

namespace Actors
{

    /// <summary>
    /// Actor that simulates traffic.
    /// </summary>
    public class StandingActor : UntypedActor
    {
        private List<Result> _results = new List<Result>();
        private List<AthleteDisqualified> _disqualifieds = new List<AthleteDisqualified>();
        private Dictionary<string,TimeSpan> _liveStanding = new Dictionary<string, TimeSpan>();
        private DateTime _raceStartedAt;

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
                case RaceCompleted ss:
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
                case Shutdown sd:
                    Handle(sd);
                    break;
            }
        }

        private void Handle(RaceStarted msg)
        {
            _raceStartedAt = msg.Timestamp;
        }

        private void Handle(AthleteEntryRegistered msg)
        {
            var racedTime = msg.Timestamp.Subtract(_raceStartedAt);
            if (_liveStanding.ContainsKey(msg.BibId))
            {
                _liveStanding[msg.BibId] = racedTime;
            }
            else
            {
                _liveStanding.Add(msg.BibId, racedTime);
            }
        }

        /// <summary>
        /// Handle StartSimulation message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(RaceCompleted msg)
        {
            var result = new Result(msg.BibId, msg.Duration);

            _results.Add(result);
        }

        private void Handle(AthleteDisqualified msg)
        {
            _disqualifieds.Add(msg);
        }

        private void Handle(Shutdown msg)
        {
            FluentConsole.White.Line("Overall standings");
            foreach (var result in _results.OrderBy(x => x.Duration).Take(3))
            {
                FluentConsole.DarkGreen.Line($"Athlete {result.BibId} completed in {result.Duration.TotalMilliseconds} ms");
            }

            FluentConsole.White.Line("Disqualifieds");
            foreach (var dis in _disqualifieds)
            {
                FluentConsole.Blue.Line($"Athlete {dis.BibId} has been disqualified {dis.GateAction} at gate {dis.Gate}");
            }

            Context.Stop(Self);
        }

        private void Handle(PrintLiveStanding msg)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Live standings at " + DateTime.Now.ToString("HH:mm:ss.ffffff"));
            foreach (var result in _liveStanding.OrderBy(x => x.Value).Take(msg.Top))
            {
                sb.AppendLine($"Athlete {result.Key} is winning in {result.Value.TotalMilliseconds} ms");
            }
            FluentConsole.DarkGreen.Line(sb.ToString());
        }


    }
}
