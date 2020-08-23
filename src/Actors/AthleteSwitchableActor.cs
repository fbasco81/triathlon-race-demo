using Akka.Actor;
using Messages;
using System;
using System.Collections.Generic;

namespace Actors
{

    /// <summary>
    /// Actor that represents a registered vehicle.
    /// </summary>
    public class AthleteSwitchableActor : ReceiveActor
    {
        string _bibId;

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

        public AthleteSwitchableActor(string bibId)
        {            
            _bibId = bibId;
            _standingActor = Context.System.ActorSelection("/user/standing");
            _bikeStandingActor = Context.System.ActorSelection("/user/standing-bike");

            //Receive<Shutdown>(
            //    msg =>
            //    {
            //        FluentConsole.Gray.Line($"Athlete #{_bibId} is shutting down.");
            //        Context.Stop(Self);
            //    });

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
                    else
                    {
                        disquilify(new AthleteDisqualified(
                            msg.BibId, msg.Timestamp,
                            msg.Gate, GateActions.Entering));
                    }

                    // flip the switch
                    Become(Swimming);
                });

            Receive<AthleteExitRegistered>(
                msg =>
                {
                    disquilify(new AthleteDisqualified(
                        msg.BibId, msg.Timestamp,
                        msg.Gate, GateActions.Exiting));
                });
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
                    else
                    {
                        disquilify(new AthleteDisqualified(
                            msg.BibId, msg.Timestamp,
                            msg.Gate, GateActions.Exiting));
                    }

                    // flip the switch
                    Become(T1);
                });

            Receive<AthleteEntryRegistered>(
                msg =>
                {
                    disquilify(new AthleteDisqualified(
                        msg.BibId, msg.Timestamp,
                        msg.Gate, GateActions.Entering));
                });
        }

        private void T1()
        {
            Receive<AthleteEntryRegistered>(
                msg =>
                {
                    if (msg.Gate == Gates.Bike)
                    {
                        _gates[msg.Gate.ToString()] = new GateInOut { In = msg.Timestamp };
                        var timeSpan = _entryTimestamp.Subtract(_gates[Gates.Swim.ToString()].Out.Value);
                        _gates["T1"] = new GateInOut { Duration = timeSpan };
                        _standingActor.Tell(msg);
                    }
                    else
                    {
                        disquilify(new AthleteDisqualified(
                            msg.BibId, msg.Timestamp,
                            msg.Gate, GateActions.Entering));
                    }

                    // flip the switch
                    Become(Biking);
                });

            Receive<AthleteExitRegistered>(
                msg =>
                {
                    disquilify(new AthleteDisqualified(
                        msg.BibId, msg.Timestamp,
                        msg.Gate, GateActions.Exiting));
                });
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
                    else
                    {
                        disquilify(new AthleteDisqualified(
                            msg.BibId, msg.Timestamp,
                            msg.Gate, GateActions.Exiting));
                    }

                    // flip the switch
                    Become(T2);
                });

            Receive<AthleteEntryRegistered>(
                msg =>
                {
                    disquilify(new AthleteDisqualified(
                        msg.BibId, msg.Timestamp,
                        msg.Gate, GateActions.Entering));
                });
        }

        private void T2()
        {
            Receive<AthleteEntryRegistered>(
                msg =>
                {
                    if (msg.Gate == Gates.Run)
                    {
                        _gates[msg.Gate.ToString()] = new GateInOut { In = msg.Timestamp };
                        var timeSpan = _entryTimestamp.Subtract(_gates[Gates.Bike.ToString()].Out.Value);
                        _gates["T2"] = new GateInOut { Duration = timeSpan };
                    }
                    else
                    {
                        disquilify(new AthleteDisqualified(
                            msg.BibId, msg.Timestamp,
                            msg.Gate, GateActions.Entering));
                    }

                    // flip the switch
                    Become(Running);
                });

            Receive<AthleteExitRegistered>(
                msg =>
                {
                    disquilify(new AthleteDisqualified(
                        msg.BibId, msg.Timestamp,
                        msg.Gate, GateActions.Entering));
                });
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

                        Become(RaceCompleted);
                    }
                    else
                    {
                        disquilify(new AthleteDisqualified(
                            msg.BibId, msg.Timestamp,
                            msg.Gate, GateActions.Exiting));
                    }

                    // flip the switch
                    Become(RaceCompleted);
                });

            Receive<AthleteEntryRegistered>(
                msg =>
                {
                    disquilify(new AthleteDisqualified(
                        msg.BibId, msg.Timestamp,
                        msg.Gate, GateActions.Entering));
                });
        }

        private void RaceCompleted()
        {
            Receive<Shutdown>(
                msg =>
                {
                    var raceFinishedAt = _gates[Gates.Swim.ToString()].Out.Value;
                    var raceCompleted = new RaceCompleted(
                        _bibId,
                        raceFinishedAt,
                        raceFinishedAt.Subtract(_gates[Gates.Swim.ToString()].In));

                    _standingActor.Tell(raceCompleted);

                    FluentConsole.Gray.Line($"Athlete #{_bibId} is shutting down.");
                    Context.Stop(Self);
                });
        }

        private void disquilify(AthleteDisqualified msg)
        {
            FluentConsole.Red.Line($"Athete #{msg.BibId} has been disqualified");
            _standingActor.Tell(msg);
            // flip the switch
            Become(RaceCompleted);
        }
    }
}
