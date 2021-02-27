using Akka.Actor;
using System;

namespace RemoteNotification
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for notifications");
            var hoConfig = Hocon.HoconConfigurationFactory.Default();

            var akkaConfig = Akka.Configuration.ConfigurationFactory.ParseString(hoConfig.ToString());

            using (ActorSystem system = ActorSystem.Create("RaceSystem", akkaConfig))
            {


                Console.Read();
            }

            

        }
    }
}
