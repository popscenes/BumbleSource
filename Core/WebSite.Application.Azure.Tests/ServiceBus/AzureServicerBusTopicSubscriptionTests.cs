using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Website.Application.Azure.Tests.ServiceBus
{
    [TestFixture]
    class AzureServicerBusTopicSubscriptionTests
    {
        public void TopicBusFactoryCreatesAzureMessageTopicBus()
        {
            var topicBusFactory = new AzureMessageTopicBusFactory();

            var topicBus = topicBusFactory.GetTopicBus("Test");

            //atm just check we return a TopicBus
            Assert.IsTrue(topicBus != null);
        }

        public void SubscriptionFactoryCreatesAzureMessageSubscriptionProcesor()
        {
            var subscriptionFactory = new AzureMessageSubscriptionFactory();

            var subscriptionProcessor = subscriptionFactory.GetSubscriptionProcessor("Testsucscription");

            //atm just check we return a subscriptionprocessor
            Assert.IsTrue(subscriptionProcessor != null);
        }
    }

    public class AzureMessageSubscriptionFactory
    {
        public AzureMessageSubscriptionProcessor GetSubscriptionProcessor(string testsucscription)
        {
            throw new NotImplementedException();
        }
    }

    public class AzureMessageSubscriptionProcessor : SubscriptionProcessorInterface
    {
    }

    public interface SubscriptionProcessorInterface
    {
    }

    public class AzureMessageTopicBusFactory
    {
        public AzureMessageTopicBus GetTopicBus(string test)
        {
            throw new NotImplementedException();
        }
    }

    public class AzureMessageTopicBus : TopicBusInterface
    {
    }

    public interface TopicBusInterface
    {
    }
}
