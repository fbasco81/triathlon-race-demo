using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using Messages;
using Actors;

namespace Host
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            //var config = ConfigurationFactory.ParseString(File.ReadAllText("akkaconfig.hocon"));

            using (ActorSystem system = ActorSystem.Create("TrafficControlSystem"))
            {
                //var roadInfo = new RoadInfo("A2", 10, 100, 5);
                var raceControlProps = Props.Create<RaceControlActor>()
                    .WithRouter(new RoundRobinPool(3));
                    //.WithRouter(new BroadcastPool(3));
                var raceControlActor = system.ActorOf(raceControlProps, "race-control");

                var entryGateActor1 = system.ActorOf<EntryGateActor>("entrygateSwim");
                var entryGateActor2 = system.ActorOf<EntryGateActor>("entrygateBike");
                var entryGateActor3 = system.ActorOf<EntryGateActor>("entrygateRun");

                var exitGateActor1 = system.ActorOf<ExitGateActor>("exitgateSwim");
                var exitGateActor2 = system.ActorOf<ExitGateActor>("exitgateBike");
                var exitGateActor3 = system.ActorOf<ExitGateActor>("exitgateRun");


                var standingActor = system.ActorOf<StandingActor>("standing");
                var bikeStandingActor = system.ActorOf<BikeStandingActor>("standing-bike");

                //var simulationProps = Props.Create<SimulationActor>().WithRouter(new BroadcastPool(3));
                var simulationProps = Props.Create<SimulationActor>().WithRouter(new RoundRobinPool(3));
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
