using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Website.Application.Messaging;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Types;

namespace Website.Application.Azure.ServiceBus
{
    public class AzureSubscriptionUtils
    {
        public List<SubscriptionDetails> GetHandlerSubscriptionsFromAssembly(Assembly assembly)
        {
            var eventInterface = typeof(EventInterface);
            var eventsTypes = TypeUtil.GetAllSubTypesFrom(eventInterface, assembly);
            var listOfSubs = new List<SubscriptionDetails>();

            foreach (var type in eventsTypes)
            {
                if (type.IsGenericType)
                {
                    var handlers =
                        assembly.DefinedTypes.Where(
                            dt =>
                            dt.ImplementedInterfaces.Any(_ => TypeUtil.NameLike(_, typeof(HandleEventInterface<>).Name) && _.GenericTypeArguments.Any(gt => TypeUtil.NameLike(gt, type.Name))));


                    listOfSubs.AddRange(handlers.Select(handler => new SubscriptionDetails() { Topic = type.Name.Replace("Event", "").Replace("`1", ""), Subscription = handler.Name.Replace("`1", ""), HandlerType = handler}));
                    continue;
                }


                Type handlerType = typeof(HandleEventInterface<>).MakeGenericType(type);
                var handlerTypes = TypeUtil.GetExpandedImplementorsUsing(handlerType, assembly);
                listOfSubs.AddRange(handlerTypes.Select(handler => new SubscriptionDetails() { Topic = type.Name.Replace("Event", ""), Subscription = handler.Name.Replace("`1", ""), HandlerType = handler }));
            }

            return listOfSubs;
        }
    }
}