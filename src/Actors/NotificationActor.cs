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

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 2,
                withinTimeRange: TimeSpan.FromSeconds(30),
                ex =>
                {
                    if (ex is UnauthorizedAccessException)
                    {
                        return Directive.Stop;
                    }
                    else if (ex is TimeoutException)
                    {
                        return Directive.Restart;
                    }
                    return OneForOneStrategy.DefaultDecider.Decide(ex);
                });
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case SendNotification ss:
                    Handle(ss);
                    break;
            }
        }

        private void Handle(SendNotification msg)
        {
            var position = 1;

            foreach (var result in msg.Results.OrderBy(x => x.Duration))
            {
                var propsPhoneNotificationActor = Props.Create<PhoneNotificationActor>();
                var phoneNotificationActor = Context.ActorOf(propsPhoneNotificationActor, $"phoneNotification-{result.BibId}");

                phoneNotificationActor.Tell(
                    new SendPhoneNotification(result.BibId, result.Duration, position)
                );
                position++;
            }
        }
    }
}
