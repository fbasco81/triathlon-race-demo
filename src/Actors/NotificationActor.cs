using Akka.Actor;
using Messages;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;
using Akka.Routing;

namespace Actors
{


    /// <summary>
    /// Actor that simulates standing.
    /// </summary>
    public class NotificationActor : UntypedActor
    {
        public NotificationActor()
        {
            
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                //case AthleteRegistered ver:
                //    Handle(ver);
                //    break;
                case SendNotification ss:
                    Handle(ss);
                    break;
                //case Shutdown sd:
                //    Handle(sd);
                //    break;
            }
        }

        //private void Handle(AthleteRegistered msg)
        //{
         //   var propsPhoneNotificationActor = Props.Create<PhoneNotificationActor>();
         //   var phoneNotificationActor = Context.ActorOf(propsPhoneNotificationActor, $"phoneNotification-{msg.BibId}");
        //}

        

        private void Handle(SendNotification msg)
        {
            var position = 1;

            foreach (var result in msg.Results.OrderBy(x => x.Duration))
            {
                //var phoneNotificationActor = Context.ActorSelection($"/user/notification/phoneNotification-{result.BibId}");

                var propsPhoneNotificationActor = Props.Create<PhoneNotificationActor>();
                var phoneNotificationActor = Context.ActorOf(propsPhoneNotificationActor, $"phoneNotification-{result.BibId}");

                phoneNotificationActor.Tell(
                    new SendPhoneNotification(result.BibId, result.Duration, position)
                );
                position++;
            }
            //Self.Tell(new Shutdown());
        }

        //private void Handle(Shutdown msg)
        //{

        //    Context.Stop(Self);
        //}
    }
}
