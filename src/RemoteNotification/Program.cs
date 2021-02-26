using Akka.Actor;
using System;

namespace RemoteNotification
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for notifications");
            Hocon.HoconConfigurationFactory.Default();

            using (ActorSystem system = ActorSystem.Create("RaceSystem"))
            {

            }

            Console.Read();

        }
    }
}
