using Akka.Actor;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Actors
{

    /// <summary>
    /// Actor that represents an athlete.
    /// </summary>
    public class AthleteSwitchableActor : ReceiveActor
    {
        string _bibId;

        private ActorSelection _standingActor;

        private Dictionary<string, GateInOut> _gates = new Dictionary<string, GateInOut>()
        {
            { Gates.Swim.ToString(), null },
            { "T1", null },
            { Gates.Bike.ToString(), null },
            { "T2", null },
            { Gates.Run.ToString(), null },
        };

        public AthleteSwitchableActor(string bibId)
        {            
            _bibId = bibId;
            _standingActor = Context.System.ActorSelection("/user/standing");

            // initialize state
            WaitingToStart();
        }

        public void WaitingToStart()
        {
            Receive<AthleteEntryRegistered>(
                msg =>
                {
                    if (msg.Gate == Gates.Swim)
                    {
                        _gates[msg.Gate.ToString()] = new GateInOut { In = msg.Timestamp };
                    }

                    // flip the switch
                    Become(Swimming);
                });

            handleRaceClosed();
        }



        public void Swimming()
        {
            Receive<AthleteExitRegistered>(
                msg =>
                {
                    if (msg.Gate == Gates.Swim)
                    {
                        _gates[msg.Gate.ToString()].Out = msg.Timestamp;
                        _gates[msg.Gate.ToString()].Duration = msg.Timestamp.Subtract(_gates[msg.Gate.ToString()].In);
                    }

                    // flip the switch
                    Become(T1);
                });

            handleRaceClosed();
        }

        private void T1()
        {
            Receive<AthleteEntryRegistered>(
                msg =>
                {
                    if (msg.Gate == Gates.Bike)
                    {
                        _gates[msg.Gate.ToString()] = new GateInOut { In = msg.Timestamp };
                        var timeSpan = msg.Timestamp.Subtract(_gates[Gates.Swim.ToString()].Out.Value);
                        _gates["T1"] = new GateInOut { Duration = timeSpan };
                        _standingActor.Tell(msg);
                    }

                    // flip the switch
                    Become(Biking);
                });

            handleRaceClosed();
        }

        private void Biking()
        {
            Receive<AthleteExitRegistered>(
                msg =>
                {
                    if (msg.Gate == Gates.Bike)
                    {
                        _gates[msg.Gate.ToString()].Out = msg.Timestamp;
                        _gates[msg.Gate.ToString()].Duration = msg.Timestamp.Subtract(_gates[msg.Gate.ToString()].In);
                    }

                    // flip the switch
                    Become(T2);
                });

            handleRaceClosed();
        }

        private void T2()
        {
            Receive<AthleteEntryRegistered>(
                msg =>
                {
                    if (msg.Gate == Gates.Run)
                    {
                        _gates[msg.Gate.ToString()] = new GateInOut { In = msg.Timestamp };
                        var timeSpan = msg.Timestamp.Subtract(_gates[Gates.Bike.ToString()].Out.Value);
                        _gates["T2"] = new GateInOut { Duration = timeSpan };
                        _standingActor.Tell(msg);
                    }
                    else
                    {
                        disqualify(new AthleteDisqualified(
                            msg.BibId, msg.Timestamp,
                            msg.Gate, GateActions.Entering));
                    }

                    // flip the switch
                    Become(Running);
                });

            handleRaceClosed();
        }

        private void Running()
        {
            Receive<AthleteExitRegistered>(
                msg =>
                {
                    if (msg.Gate == Gates.Run)
                    {
                        _gates[msg.Gate.ToString()].Out = msg.Timestamp;
                        _gates[msg.Gate.ToString()].Duration = msg.Timestamp.Subtract(_gates[msg.Gate.ToString()].In);

                        Self.Tell(new Shutdown());
                    }

                    _standingActor.Tell(new AthleteRaceResult(_bibId, _gates));

                    // flip the switch
                    Become(RaceCompleted);
                });

            handleRaceClosed();
        }

        private void RaceCompleted()
        {
            Receive<Shutdown>(
                msg =>
                {
                    var missedGate = missGates();
                    if (missedGate != null)
                    {
                        disqualify( new AthleteDisqualified(
                                _bibId, DateTime.Now, missedGate.Gate, missedGate.GateAction));
                    }
                    else
                    {
                        var raceFinishedAt = _gates[Gates.Run.ToString()].Out.Value;
                        var raceCompleted = new AthleteRaceCompleted(
                            _bibId,
                            raceFinishedAt,
                            raceFinishedAt.Subtract(_gates[Gates.Swim.ToString()].In));

                        _standingActor.Tell(raceCompleted);
                    }

                    shutdown();
                });
        }

        private void Disqualified()
        {
            Receive<Shutdown>(
                msg =>
                {
                    var missedGate = missGates();
                    if (missedGate != null)
                    {
                        disqualify(new AthleteDisqualified(
                                _bibId, DateTime.Now, missedGate.Gate, missedGate.GateAction));
                    }

                    shutdown();
                });
        }

        private void handleRaceClosed()
        {
            //TODO: handle Shutdown for each state
            Receive<RaceClosed>(
                msg =>
                {
                    Self.Tell(new Shutdown());
                    Become(Disqualified);
                });
        }

        private void disqualify(AthleteDisqualified msg)
        {
            FluentConsole.Red.Line($"Athete #{msg.BibId} has been disqualified");
            _standingActor.Tell(msg);
            // flip the switch
            Become(RaceCompleted);
        }

        private void shutdown()
        {
            Context.Stop(Self);
        }

        private MissedGateInfo missGates()
        {
            foreach(var kv in _gates.Where(x => x.Key != "T1" && x.Key != "T2"))
            {
                if (kv.Value == null)
                {
                   return new MissedGateInfo( (Gates)Enum.Parse(typeof(Gates), kv.Key, true), GateActions.Entering);
                }
                else if(kv.Value.Out == null)
                {
                   return new MissedGateInfo((Gates)Enum.Parse(typeof(Gates), kv.Key, true), GateActions.Exiting);
                }
            }

            return null;
        }
    }

    public class MissedGateInfo
    {
        public Gates Gate { get; private set; }
        public GateActions GateAction { get; private set; }

        public MissedGateInfo(Gates gate, GateActions gateAction)
        {
            Gate = gate;
            GateAction = gateAction;
        }
    }
}
