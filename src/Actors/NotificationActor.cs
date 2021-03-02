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
        IActorRef _phoneNotificationActor;
        public NotificationActor()
        {
            var phoneNotificationProps = Props.Create<PhoneNotificationActor>()
                .WithRouter(new RoundRobinPool(1));

            _phoneNotificationActor = Context.ActorOf(phoneNotificationProps, "phoneNotification");
        }

       
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case SendNotification ss:
                    Handle(ss);
                    break;
                case Shutdown sd:
                    Handle(sd);
                    break;
            }
        }

        private void Handle(SendNotification msg)
        {
            var position = 1;
            foreach (var result in msg.Results.OrderBy(x => x.Duration))
            {
                _phoneNotificationActor.Tell(
                    new SendPhoneNotification(result.BibId, result.Duration, position)
                );
                position++;
            }
            //Self.Tell(new Shutdown());
        }

        private void Handle(Shutdown msg)
        {
            
            Context.Stop(Self);
        }
    }
}
