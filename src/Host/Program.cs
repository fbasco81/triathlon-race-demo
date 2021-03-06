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
            
            var akkaConfig = Akka.Configuration.ConfigurationFactory.Default();
                        
            var gates = new Dictionary<Gates, GateInfo>()
            {
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

            using (ActorSystem system = ActorSystem.Create("RaceSystem", akkaConfig))
            {
                 var raceControlProps = Props.Create<RaceControlActor>()
                    .WithRouter(new RoundRobinPool(3));
                 var raceControlActor = system.ActorOf(raceControlProps, "race-control");

                foreach(var kv in gates)
                {
                    var entrygate = "entrygate" + kv.Key.ToString().ToLower();
                    system.ActorOf<EntryGateActor>(entrygate);

                    var exitgate = "exitgate" + kv.Key.ToString().ToLower();
                    system.ActorOf<ExitGateActor>(exitgate);

                    for (int i = 0; i < kv.Value.NrOfIntermediateChecks; i++)
                    {
                        var n = $"intermediategate-{kv.Key.ToString().ToLower()}-{i + 1}";
                        system.ActorOf<IntermediateGateActor>(n);
                    }
                }

                var standingActorProps = Props.Create<StandingActor>();
                var standingActor = system.ActorOf(standingActorProps, "standing");

                var simulationProps = Props.Create<SimulationActor>(gates).WithRouter(new RoundRobinPool(3));
                var simulationActor = system.ActorOf(simulationProps);

                Console.WriteLine("Actorsystem and actor created. Press any key to start simulation\n");
                Console.ReadKey(true);

                simulationActor.Tell(new StartSimulation(100));
                
                Console.ReadKey(true);
                system.Terminate();

                System.Console.WriteLine("Stopped. Press any key to exit.");
                Console.ReadKey(true);
            }
        }
    }
}
