using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using Messages;
using Actors;
using System.Collections.Generic;

namespace Host
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            var gates = new Dictionary<Gates, GateInfo>()
            {
                //{ Gates.Swim.ToString(), new ExitDelayInSecond{Min = 8, Max = 16} },
                //{ Gates.Bike.ToString(), new ExitDelayInSecond{Min = 25, Max = 40} },
                //{ Gates.Run.ToString(), new ExitDelayInSecond{Min = 14, Max = 30} },
                { Gates.Swim, new GateInfo(){
                    ExitDelay = new ExitDelayInSecond{Min = 1, Max = 2},
                    NrOfIntermediateChecks = 0 } },
                { Gates.Bike,
                    new GateInfo(){ ExitDelay = new ExitDelayInSecond{Min = 3, Max = 4},
                        NrOfIntermediateChecks = 2 } },
                { Gates.Run,  new GateInfo(){
                    ExitDelay = new ExitDelayInSecond{Min = 2, Max = 3},
                    NrOfIntermediateChecks = 1 } },
            };

            //var config = ConfigurationFactory.ParseString(File.ReadAllText("akkaconfig.hocon"));

            using (ActorSystem system = ActorSystem.Create("RaceSystem"))
            {
                //var roadInfo = new RoadInfo("A2", 10, 100, 5);
                var raceControlProps = Props.Create<RaceControlActor>()
                    .WithRouter(new RoundRobinPool(3));
                    //.WithRouter(new BroadcastPool(3));
                var raceControlActor = system.ActorOf(raceControlProps, "race-control");

                foreach(var kv in gates)
                {
                    var entrygate = "entrygate" + kv.Key.ToString().ToLower();
                    //Console.WriteLine("Creating actor {0}", entrygate);
                    system.ActorOf<EntryGateActor>(entrygate);

                    var exitgate = "exitgate" + kv.Key.ToString().ToLower();
                    //Console.WriteLine("Creating actor {0}", exitgate);
                    system.ActorOf<ExitGateActor>(exitgate);

                    for (int i = 0; i < kv.Value.NrOfIntermediateChecks; i++)
                    {
                        var n = $"intermediategate-{kv.Key.ToString().ToLower()}-{i + 1}";
                       // Console.WriteLine("Creating actor {0}", n);
                        system.ActorOf<IntermediateGateActor>(n);
                    }
                }


                var standingActor = system.ActorOf<StandingActor>("standing");
                var bikeStandingActor = system.ActorOf<BikeStandingActor>("standing-bike");

                //var simulationProps = Props.Create<SimulationActor>().WithRouter(new BroadcastPool(3));
                var simulationProps = Props.Create<SimulationActor>(gates).WithRouter(new RoundRobinPool(3));
                var simulationActor = system.ActorOf(simulationProps);

                Console.WriteLine("Actorsystem and actor created. Press any key to start simulation\n");
                Console.ReadKey(true);

                simulationActor.Tell(new StartSimulation(100));
                //simulationActor.Tell(new TestSimulation(18));

                Console.ReadKey(true);
                system.Terminate();

                System.Console.WriteLine("Stopped. Press any key to exit.");
                Console.ReadKey(true);
            }
        }
    }
}
