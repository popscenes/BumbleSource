using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using Ninject;
using Website.Application.ApplicationCommunication;
using Website.Application.Azure.Communication;
using Website.Azure.Common.TableStorage;
using Website.Test.Common;

namespace Website.Application.Azure.Tests.Communication
{
    [TestFixture]
    public class AzureApplicationBroadcastCommunicatorRegistrationTests
    {

        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
//            Kernel.Rebind<TableNameAndPartitionProviderInterface>()
//                .ToConstant(new TableNameAndPartitionProvider<SimpleExtendableEntity>()
//                            {
//                                {typeof(SimpleExtendableEntity), 0, "broadcastCommunicatorsTest", e => "", e => e.Get<string>("Endpoint")}                       
//                            })
//                .WhenAnyAnchestorNamed("broadcastCommunicators");

            _tableName =
                Kernel.Get<TableNameAndPartitionProviderServiceInterface>().GetTableName<AzureBroadcastRegistrationEntry>();
            Reinit();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {

        }

        private string _tableName;
        private void Reinit()   
        {
            var cloudTable = Kernel.Get<CloudTableClient>();
            var tabRef = cloudTable.GetTableReference(_tableName);
            tabRef.DeleteIfExists();

            var ctx = Kernel.Get<TableContextInterface>();
            ctx.InitTable<SimpleExtendableEntity>(_tableName);

//            Kernel.Get<AzureTableContext>("broadcastCommunicators").InitFirstTimeUse();      
        }


        [Test]
        public void AzureBroadcastRegistratorTestRegistration()
        {
            Reinit();
            var registrator = Kernel.Get<ApplicationBroadcastCommunicatorRegistrationInterface>();
            registrator.RegisterEndpoint("blah");
            registrator.RegisterEndpoint("test");

            var ctx = Kernel.Get<TableContextInterface>();


            var registrations = ctx
                .PerformQuery<AzureBroadcastRegistrationEntry>(_tableName)
                .Select(e => e.Get<string>("Endpoint"))
                .ToList();

            CollectionAssert.Contains(registrations, "blah");
            CollectionAssert.Contains(registrations, "test");
            AssertUtil.Count(2, registrations);

            var retregistrations = registrator.GetCurrentEndpoints();
            CollectionAssert.Contains(retregistrations, "blah");
            CollectionAssert.Contains(retregistrations, "test");
            AssertUtil.Count(2, retregistrations);
        }

        [Test]
        public void AzureBroadcastRegistratorTestReRegistrationUpdatesLastRegisterTime()
        {
            Reinit();
            var registrator = Kernel.Get<ApplicationBroadcastCommunicatorRegistrationInterface>();
            registrator.RegisterEndpoint("blah");
            registrator.RegisterEndpoint("test");

            var ctx = Kernel.Get<TableContextInterface>();


            var registrations = ctx
                .PerformQuery<AzureBroadcastRegistrationEntry>(_tableName)
                .ToList();
            //fake some elapsed time
            var testReg = registrations.Single(r => r.Get<string>("Endpoint") == "blah");
            testReg["LastRegisterTime"] = DateTime.UtcNow.AddMinutes(-20);
            ctx.Store("broadcastCommunicatorsTest", testReg);
            testReg = registrations.Single(r => r.Get<string>("Endpoint") == "test");
            testReg["LastRegisterTime"] = DateTime.UtcNow.AddMinutes(-20);
            ctx.Store("broadcastCommunicatorsTest", testReg);
            ctx.SaveChanges();

            registrations = ctx
                .PerformQuery<AzureBroadcastRegistrationEntry>(_tableName)
                .ToList();

            var regTimeBlah = registrations.Single(r => r.Get<string>("Endpoint") == "blah")["LastRegisterTime"];
            var regTimeTest = registrations.Single(r => r.Get<string>("Endpoint") == "test")["LastRegisterTime"];

            registrator = Kernel.Get<ApplicationBroadcastCommunicatorRegistrationInterface>();
            registrator.RegisterEndpoint("blah");
            registrator.RegisterEndpoint("test");

            registrations = ctx
                .PerformQuery<AzureBroadcastRegistrationEntry>(_tableName)
                .ToList();
            var regTimeBlahNow = registrations.Single(r => r.Get<string>("Endpoint") == "blah")["LastRegisterTime"];
            var regTimeTestNow = registrations.Single(r => r.Get<string>("Endpoint") == "test")["LastRegisterTime"];

            Assert.That(regTimeBlahNow, Is.GreaterThan(regTimeBlah));
            Assert.That(regTimeBlahNow, Is.GreaterThan(regTimeTest));

            var registrationNames = ctx
                .PerformQuery<AzureBroadcastRegistrationEntry>(_tableName)
                .Select(e => e.Get<string>("Endpoint"))
                .ToList();
            CollectionAssert.Contains(registrationNames, "blah");
            CollectionAssert.Contains(registrationNames, "test");
            Assert.That(registrations.Count(), Is.EqualTo(2));
        }

        [Test]
        public void AzureBroadcastRegistratorTestRegistrationClearsInactiveBroadcastEndpoints()
        {
            Reinit();
            var registrator = Kernel.Get<ApplicationBroadcastCommunicatorRegistrationInterface>();           
            registrator.RegisterEndpoint("test");

            var ctx = Kernel.Get<TableContextInterface>();

            var registrations = ctx
                .PerformQuery<AzureBroadcastRegistrationEntry>(_tableName)
                .ToList();

            var testReg = registrations.Single(r => r.Get<string>("Endpoint") == "test");
            testReg["LastRegisterTime"] = DateTime.UtcNow.AddMinutes(-20);
            ctx.Store("broadcastCommunicatorsTest", testReg);
            ctx.SaveChanges();

            registrator = Kernel.Get<ApplicationBroadcastCommunicatorRegistrationInterface>();
            registrator.RegisterEndpoint("blah");

            var registrationNames = ctx
                .PerformQuery<AzureBroadcastRegistrationEntry>(_tableName)
                .Select(e => e.Get<string>("Endpoint"))
                .ToList();
            CollectionAssert.Contains(registrationNames, "blah");
            CollectionAssert.DoesNotContain(registrationNames, "test");
            AssertUtil.Count(1, registrationNames);

        }
    }
}
