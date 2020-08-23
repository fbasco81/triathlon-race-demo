using Akka.Actor;
using Messages;
using System;
using System.Collections.Generic;

namespace Actors
{
    public class GateInOut
    {
        public DateTime In { get; set; }
        public DateTime? Out { get; set; }
        public TimeSpan? Duration { get; set; }
    }

    /// <summary>
    /// Actor that represents a registered vehicle.
    /// </summary>
    public class AthleteActor : UntypedActor
    {
        string _bibId;
        string _brand = "Unknown color";
        string _color = "Unknown brand";
        DateTime _firstEntryTimestamp;
        DateTime _entryTimestamp;
        DateTime? _exitTimestamp;

        //private RoadInfo _roadInfo;

        double _elapsedMinutes;
        double _avgSpeedInKmh;

        private ActorSelection _standingActor;
        private ActorSelection _bikeStandingActor;

        private Dictionary<string, GateInOut> _gates = new Dictionary<string, GateInOut>()
        {
            { Gates.Swim.ToString(), null },
            { "T1", null },
            { Gates.Bike.ToString(), null },
            { "T2", null },
            { Gates.Run.ToString(), null },
        };

        public AthleteActor(string bibId)
        {
            // initialize state
            //_roadInfo = roadInfo;
            _bibId = bibId;
            _standingActor = Context.System.ActorSelection("/user/standing");
            _bikeStandingActor = Context.System.ActorSelection("/user/standing-bike");
        }

        /// <summary>
        /// Handle received message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case AthleteEntryRegistered ver:
                    Handle(ver);
                    break;
                case AthleteExitRegistered vxr:
                    Handle(vxr);
                    break;            
                //case VehicleInfoAvailable via:
                //    Handle(via);
                //    break; 
                //case MissingCarDetected mcd:
                //    Handle(mcd);
                //    break;                        
                case Shutdown sd:
                    Handle(sd);
                    break;                        
            }
        }         



        /// <summary>
        /// Handle VehicleEntryRegistered message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(AthleteEntryRegistered msg)
        {
            //FluentConsole.Green.Line($"Vehicle '{msg.VehicleId}' entered at {msg.Timestamp.ToString("HH:mm:ss.ffffff")}");

            _gates[msg.Gate.ToString()] = new GateInOut { In = msg.Timestamp };

            if (msg.Gate == Gates.Bike)
            {
                var timeSpan = _entryTimestamp.Subtract(_gates[Gates.Swim.ToString()].Out.Value);
                _gates["T1"] = new GateInOut { Duration = timeSpan };
            }

            if (msg.Gate == Gates.Run)
            {
                var timeSpan = _entryTimestamp.Subtract(_gates[Gates.Bike.ToString()].Out.Value);
                _gates["T2"] = new GateInOut { Duration = timeSpan };
            }


            //if (_gates[Gates.Swim.ToString()] == null)
            //{
            //    _firstEntryTimestamp = msg.Timestamp;
            //    gate = Gates.Swim.ToString();
            //}
            //else if (_gates[Gates.Bike.ToString()] == null)
            //{
            //    _gates["T1"] = _entryTimestamp.Subtract(_exitTimestamp.Value).TotalSeconds;

            //    gate = Gates.Bike.ToString();
            //}
            //else if (_gates[Gates.Run.ToString()] == null)
            //{
            //    _gates["T2"] = _entryTimestamp.Subtract(_exitTimestamp.Value).TotalSeconds;


            //    gate = Gates.Run.ToString();
            //}
            //else
            //{
            //    throw new ApplicationException("Unexpected enter registered");
            //}

            Console.WriteLine("Athlete {0} entered gate {1}", msg.BibId, msg.Gate);

            //_dmvActor = Context.ActorOf<DMVActor>();
            //_dmvActor.Tell(new GetVehicleInfo(_bibId));

            //// set a time-out after wich we consider a car missing
            //Context.System.Scheduler.ScheduleTellOnce(
            //    TimeSpan.FromMinutes(5), Self, new MissingCarDetected(msg.VehicleId), Self);


        }

        /// <summary>
        /// Handle VehicleInfoAvailable message.
        /// </summary>
        /// <param name="msg">Message to handle.</param>
        //private void Handle(VehicleInfoAvailable msg)
        //{
        //    _brand = msg.Brand;
        //    _color = msg.Color;
        //}

        /// <summary>
        /// Handle VehicleExitRegistered message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(AthleteExitRegistered msg)
        {

            _gates[msg.Gate.ToString()].Out = msg.Timestamp;
            _gates[msg.Gate.ToString()].Duration = msg.Timestamp.Subtract(_gates[msg.Gate.ToString()].In);

            //if (_gates[Gates.Swim.ToString()] == null)
            //{
            //    _gates[Gates.Swim.ToString()] = _exitTimestamp.Value.Subtract(_entryTimestamp).TotalSeconds;
            //    gate = Gates.Swim.ToString();
            //    var swimCompleted = new SwimCompleted(
            //        msg.BibId,
            //        msg.Timestamp.Subtract(_entryTimestamp));
            //}
            //else if (_gates[Gates.Bike.ToString()] == null)
            //{
            //    _gates[Gates.Bike.ToString()] = _exitTimestamp.Value.Subtract(_entryTimestamp).TotalSeconds;
            //    gate = Gates.Bike.ToString();
            //    var bikeCompleted = new BikeCompleted(
            //        msg.BibId,
            //        msg.Timestamp.Subtract(_entryTimestamp));
            //    _bikeStandingActor.Tell(bikeCompleted);
            //}
            //else if (_gates[Gates.Run.ToString()] == null)
            //{
            //    _gates[Gates.Run.ToString()] = _exitTimestamp.Value.Subtract(_entryTimestamp).TotalSeconds;
            //    gate = Gates.Run.ToString();

            //    var runCompleted = new RunCompleted(
            //        msg.BibId,
            //        msg.Timestamp.Subtract(_entryTimestamp));

            //    var raceCompleted = new RaceCompleted(
            //        msg.BibId,
            //        msg.Timestamp,
            //        msg.Timestamp.Subtract(_firstEntryTimestamp));

            //    _standingActor.Tell(raceCompleted);
            //    _bikeStandingActor.Tell(raceCompleted);

            //    Self.Tell(new Shutdown());
            //}
            //else
            //{
            //    throw new ApplicationException("Unexpected exit registered");
            //}

            if(msg.Gate == Gates.Run)
            {
                var raceCompleted = new RaceCompleted(
                    msg.BibId,
                    msg.Timestamp,
                    msg.Timestamp.Subtract(_gates[Gates.Swim.ToString()].In));

                _standingActor.Tell(raceCompleted);

                Self.Tell(new Shutdown());
            }

            Console.WriteLine("Athlete {0} exited gate {1}", msg.BibId, msg.Gate);
        }

        /// <summary>
        /// Handle Shutdown message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void Handle(Shutdown msg)
        {
            //if (_dmvActor != null)
            //{
            //    Context.Stop(_dmvActor);
            //}
            Context.Stop(Self);
        }

        /// <summary>
        /// Handle MissingCarDetected message.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        //private void Handle(MissingCarDetected msg)
        //{
        //    FluentConsole.Magenta.Line($"Vehicle '{msg.VehicleId}' is missing. Sending road assistance.");

        //    // ...
        //}

        #region Private helper methods

        /// <summary>
        /// Determine whether or not the vehicle was speeding.
        /// </summary>
        /// <returns>Violation in Km/h after correction.</returns>
        //private int DetermineSpeedingViolation()
        //{
        //    //_elapsedMinutes = _exitTimestamp.Value.Subtract(_entryTimestamp).TotalMinutes;
        //    _elapsedMinutes = _exitTimestamp.Value.Subtract(_entryTimestamp).TotalSeconds; // 1 sec. == 1 min. in simulation
        //    _avgSpeedInKmh = Math.Round((_roadInfo.SectionLengthInKm / _elapsedMinutes) * 60);
        //    int violation = Convert.ToInt32(_avgSpeedInKmh - _roadInfo.MaxAllowedSpeedInKmh - _roadInfo.LegalCorrectionInKmh);
        //    return violation;
        //}

        #endregion
    }
}
