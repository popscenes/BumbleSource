using System;
using System.Linq;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Intergrations;
using Website.Test.Common.Facebook;

namespace Website.Application.Tests.Intergrations
{
    [TestFixture]    
    public class FacebookIntergrationTests
    {
        protected FacebookTestUser testUser;
        
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Facebookutils.TestUserAdd("Heavy Metal Kid", "user_events,friends_events,publish_stream,create_event");
            testUser = Facebookutils.TestUserGet();
            Facebookutils.TestEventAdd(testUser.access_token);
            Facebookutils.TestEventAdd(testUser.access_token);
            Facebookutils.TestEventAdd(testUser.access_token);


        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Facebookutils.TestUserDelete(testUser.id);
        }

        [Test]
        public void FacebookIntergrationTestGetUser()
        {
            var graphApi = new FacebookGraph(testUser.access_token);
            var fbUser = graphApi.GetUser();

            Assert.AreEqual(fbUser.name, "Heavy Metal Kid");
            Assert.AreEqual(fbUser.first_name, "Heavy");
            Assert.AreEqual(fbUser.middle_name, "Metal");
            Assert.AreEqual(fbUser.last_name, "Kid");
        }

        [Test]
        public void FacebookIntergrationTestPublishUserStatus()
        {
            var graphApi = new FacebookGraph(testUser.access_token);
            var id = graphApi.PublishStatus("http://bumble.cloudapp.net/", "Bumble all the way", "http://bumble.cloudapp.net/content/themes/taskflya/images/postaflya-logo.png");
            Assert.IsFalse(String.IsNullOrWhiteSpace(id));
            
        }

        [Test]
        public void FacebookIntergrationTestPermissionsGet()
        {
            var graphApi = new FacebookGraph(testUser.access_token);
            var perms = graphApi.GetUserPermission();
            Assert.IsTrue(perms[0]["user_events"]);
            Assert.IsTrue(perms[0]["friends_events"]);
            Assert.IsTrue(perms[0]["publish_stream"]);
            Assert.IsTrue(perms[0]["create_event"]);
            Assert.IsTrue(perms[0]["installed"]);

        }

        [Test]
        public void FacebookIntergrationTestUserEventsGet()
        {
            var graphApi = new FacebookGraph(testUser.access_token);
            var eventsList = graphApi.UserEventsGet();

            Assert.That(eventsList.Count(), Is.EqualTo(3));

            Assert.AreEqual(eventsList[0].name, "Test Event 1");
            Assert.AreEqual(eventsList[0].description, "this is a test event yo");
            Assert.AreEqual(eventsList[0].start_time.ToString("yyyy-MM-dd"), DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"));
            Assert.AreEqual(eventsList[0].end_time.ToString("yyyy-MM-dd"), DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"));
            Assert.AreEqual(eventsList[0].location, "After The Tears");
            Assert.IsTrue(Math.Abs(eventsList[0].venue.location.latitude - -37.8839340209961) < 0.0001);
            Assert.IsTrue(Math.Abs(eventsList[0].venue.location.longitude - 145.0004344094) < 0.0001);
            Assert.AreEqual(eventsList[0].privacy, "OPEN");
            Assert.IsTrue(Uri.IsWellFormedUriString(eventsList[0].picture, UriKind.Absolute));
        }


    }
}
