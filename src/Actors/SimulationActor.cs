using Akka.Actor;
using Messages;
using System;
using System.Collections.Generic;

namespace Actors
{
    public class ExitDelayInSecond
    {
        public int Min{ get; set; }

        public int Max{ get; set; }
    }

    /// <summary>
    /// Actor that simulates traffic.
    /// </summary>
    public class SimulationActor : UntypedActor
    {
        private int _numberOfAthletes;
        private int _atheltesSimulated;
        private string _randomWinner;
        private string _randomDisqualified;
        private Random _rnd;
        private TimeSpan _raceDuration = TimeSpan.FromSeconds(20);

        private int _minEntryDelayInMS = 50;
        private int _maxEntryDelayInMS = 5000;
        private int _minTransitionDelayInS = 1;
        private int _maxTransitionDelayInS = 2;
        private Dictionary<Gates, ExitDelayInSecond> _exitDelay = new Dictionary<Gates, ExitDelayInSecond>()
        {
            //{ Gates.Swim.ToString(), new ExitDelayInSecond{Min = 8, Max = 16} },
            //{ Gates.Bike.ToString(), new ExitDelayInSecond{Min = 25, Max = 40} },
            //{ Gates.Run.ToString(), new ExitDelayInSecond{Min = 14, Max = 30} },
            { Gates.Swim, new ExitDelayInSecond{Min = 1, Max = 2} },
            { Gates.Bike, new ExitDelayInSecond{Min = 3, Max = 4} },
            { Gates.Run, new ExitDelayInSecond{Min = 2, Max = 3} },
        };

        /// <summary>
        /// Handle received message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case TestSimulation ss:
                    Handle(ss);
                    break;
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

        private void Handle(TestSimulation msg)
        {
            var raceControl = Context.System.ActorSelection($"/user/race-control");


            for (int i = 0; i < msg.NumberOfAthletes; i++)
            {
                raceControl.Tell(new Test(i.ToString()));
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
            _atheltesSimulated = 0;
            _rnd = new Random();

            // start simulationloop
            //var simulatePassingCar = new SimulatePassingCar(GenerateRandomLicenseNumber());
            //Context.System.Scheduler.ScheduleTellOnce(
            //    _rnd.Next(_minEntryDelayInMS, _maxEntryDelayInMS), Self, simulatePassingCar, Self);

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

            var standingActor = Context.System.ActorSelection($"/user/standing");
            standingActor.Tell(new RaceStarted(raceStartedAt));

            var liveStandingScheduler = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1),
                standingActor,
                new PrintLiveStanding(3),
                Self);
            liveStandingScheduler.CancelAfter(TimeSpan.FromSeconds(7));//_raceDuration - _raceDuration.Subtract(TimeSpan.FromMilliseconds(_raceDuration.TotalMilliseconds / 10))

            for (int i = 0; i < msg.NumberOfAthletes; i++)
            {
                var bibId = (i + 1).ToString();
                Self.Tell(new SimulatePassingAthlete(bibId, raceStartedAt));
            }

            var raceClosed = new RaceClosed();
            var standingBikeActor = Context.System.ActorSelection($"/user/standing-bike");
            Context.System.Scheduler.ScheduleTellOnce(_raceDuration.Add(TimeSpan.FromSeconds(1)), standingBikeActor, new Shutdown(), Self);

            Context.System.Scheduler.ScheduleTellOnce(_raceDuration, 
                standingActor, 
                new Shutdown(), 
                Self);

        }

        private void Handle(SimulatePassingAthlete msg)
        {
            _atheltesSimulated++;
            var isWinner = msg.BibId == _randomWinner;

            DateTime entryTimestamp = DateTime.Now;// msg.RaceStartedAt;
            TimeSpan delay = TimeSpan.FromSeconds(0);
            var counter = 1;
            foreach (var kv in _exitDelay)
            {
                var athletePasedAsEntered = new AthletePassed(msg.BibId, entryTimestamp, kv.Key);
                ActorSelection entryGate = Context.System.ActorSelection($"/user/entrygate{kv.Key}");
                Context.System.Scheduler.ScheduleTellOnce(delay, entryGate, athletePasedAsEntered, Self);
                //Console.WriteLine("Athlete {0} entered gate {1} w/ delay {2}", msg.BibId, counter, delay.TotalSeconds);

                var gateDelay = !isWinner ?
                    TimeSpan.FromSeconds(_rnd.Next(kv.Value.Min, kv.Value.Max) + _rnd.NextDouble())
                    : TimeSpan.FromSeconds(kv.Value.Min);

                delay = delay + gateDelay;
                DateTime exitTimestamp = entryTimestamp.Add(delay);
                var athletePasedAsExited = new AthletePassed(msg.BibId, exitTimestamp, kv.Key);
                ActorSelection exityGate = Context.System.ActorSelection($"/user/exitgate{kv.Key}");
                if (msg.BibId ==_randomDisqualified)
                {
                    FluentConsole.Magenta.Line($"Athlete #{_randomDisqualified} should be disqualified for missing exiting gate {kv.Key}");
                }
                else
                {
                    Context.System.Scheduler.ScheduleTellOnce(delay, exityGate, athletePasedAsExited, Self);
                    //Console.WriteLine("Athlete {0} exited gate {1} w/ delay {2}", msg.BibId, counter, delay.TotalSeconds);
                }

                if (counter < _exitDelay.Count)
                {
                    var transitionTime = !isWinner ? 
                        TimeSpan.FromSeconds(_rnd.Next(_minTransitionDelayInS, _minTransitionDelayInS) + _rnd.NextDouble())
                        : TimeSpan.FromSeconds(_minTransitionDelayInS);
                    entryTimestamp = exitTimestamp.Add(transitionTime);
                    delay = delay + transitionTime;
                }

                counter++;
            }

            if (_atheltesSimulated == _numberOfAthletes)
            {
                Self.Tell(new Shutdown());
            }
        }

        /// <summary>
        /// Handle SimulatePassingCar message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        //private void Handle(SimulatePassingCar msg)
        //{
        //    //  simulate car entry
        //    int entryLane = _rnd.Next(1, 4);
        //    ActorSelection entryCamera = Context.System.ActorSelection($"/user/entrycam{entryLane}");
        //    DateTime entryTimestamp = DateTime.Now;
        //    VehiclePassed vehiclePassed = new VehiclePassed(msg.VehicleId, entryTimestamp);
        //    entryCamera.Tell(vehiclePassed);

        //    // simulate car exit
        //    int exitLane = _rnd.Next(1, 4);
        //    TimeSpan delay = TimeSpan.FromSeconds(_rnd.Next(_minExitDelayInS, _maxExitDelayInS) + _rnd.NextDouble());
        //    DateTime exitTimestamp = entryTimestamp.Add(delay);
        //    ActorSelection exitCamera = Context.System.ActorSelection($"/user/exitcam{entryLane}");
        //    vehiclePassed = new VehiclePassed(msg.VehicleId, exitTimestamp);
        //    Context.System.Scheduler.ScheduleTellOnce(delay, exitCamera, vehiclePassed, Self);

        //    // handle progress
        //    _carsSimulated++;
        //    if (_carsSimulated < _numberOfCars)
        //    {
        //        SimulatePassingCar simulatePassingCar = new SimulatePassingCar(GenerateRandomLicenseNumber());
        //        Context.System.Scheduler.ScheduleTellOnce(
        //            _rnd.Next(_minEntryDelayInMS, _maxEntryDelayInMS), Self, simulatePassingCar, Self);
        //    }
        //    else
        //    {
        //        Self.Tell(new Shutdown());
        //    }
        //}

        #region Private helper methods

        private string _validLicenseNumberChars = "DFGHJKLNPRSTXYZ";

        /// <summary>
        /// Generate random licensenumber.
        /// </summary>
        private string GenerateRandomLicenseNumber()
        {
            int type = _rnd.Next(1, 9);
            string kenteken = null;
            switch (type)
            {
                case 1: // 99-AA-99
                    kenteken = string.Format("{0:00}-{1}-{2:00}", _rnd.Next(1, 99), GenerateRandomCharacters(2), _rnd.Next(1, 99));
                    break;
                case 2: // AA-99-AA
                    kenteken = string.Format("{0}-{1:00}-{2}", GenerateRandomCharacters(2), _rnd.Next(1, 99), GenerateRandomCharacters(2));
                    break;
                case 3: // AA-AA-99
                    kenteken = string.Format("{0}-{1}-{2:00}", GenerateRandomCharacters(2), GenerateRandomCharacters(2), _rnd.Next(1, 99));
                    break;
                case 4: // 99-AA-AA
                    kenteken = string.Format("{0:00}-{1}-{2}", _rnd.Next(1, 99), GenerateRandomCharacters(2), GenerateRandomCharacters(2));
                    break;
                case 5: // 99-AAA-9
                    kenteken = string.Format("{0:00}-{1}-{2}", _rnd.Next(1, 99), GenerateRandomCharacters(3), _rnd.Next(1, 10));
                    break;
                case 6: // 9-AAA-99
                    kenteken = string.Format("{0}-{1}-{2:00}", _rnd.Next(1, 9), GenerateRandomCharacters(3), _rnd.Next(1, 10));
                    break;
                case 7: // AA-999-A
                    kenteken = string.Format("{0}-{1:000}-{2}", GenerateRandomCharacters(2), _rnd.Next(1, 999), GenerateRandomCharacters(1));
                    break;
                case 8: // A-999-AA
                    kenteken = string.Format("{0}-{1:000}-{2}", GenerateRandomCharacters(1), _rnd.Next(1, 999), GenerateRandomCharacters(2));
                    break;
            }

            return kenteken;
        }

        private string GenerateRandomCharacters(int aantal)
        {
            char[] chars = new char[aantal];
            for (int i = 0; i < aantal; i++)
            {
                chars[i] = _validLicenseNumberChars[_rnd.Next(_validLicenseNumberChars.Length - 1)];
            }
            return new string(chars);
        }

        #endregion
    }
}
