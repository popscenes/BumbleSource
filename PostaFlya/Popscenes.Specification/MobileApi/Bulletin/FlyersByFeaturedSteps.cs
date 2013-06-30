using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Popscenes.Specification.Util;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Areas.MobileApi.Infrastructure.Model;
using PostaFlya.Domain.Flier;
using TechTalk.SpecFlow;

namespace Popscenes.Specification.MobileApi.Bulletin
{
    [Binding]
    public class FlyersByFeaturedSteps : Steps
    {
        // For additional details on SpecFlow step definitions see http://go.specflow.org/doc-stepdef

        [Then(@"The content should contain a list of (.*) flyers ordered by created date desc")]
        public void ThenTheContentShouldContainAListOfFlyersOrderedByCreatedDateDesc(int takenumber)
        {
            var content = SpecUtil.GetResponseContentAs<ResponseContent<FlyerSummaryContent>>();
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Data, Is.Not.Null);
            Assert.That(content.Data.Flyers, Is.Not.Null);
            Assert.That(content.Data.Flyers.Count, Is.EqualTo(takenumber));

            var prev = default(DateTime);
            foreach (var curr in content.Data.Flyers.Select(f => StorageUtil.Get<Flier>(f.Id).CreateDate).Where(curr => prev != default(DateTime)))
            {
                Assert.That(prev, Is.GreaterThanOrEqualTo(curr));
                prev = curr;
            }
        }

        [Given(@"I have retrieved the latest (.*) flyers using (.*)")]
        public void GivenIHaveRetrievedTheLatestFlyers(int takenumber, string requestFormat)
        {
            When(string.Format(@"I perform a get request for the path " + requestFormat, takenumber));
            Then(@"I should receive a http response with a status of 200");
            Then(@"The content should have a response status of OK");
            Then(string.Format(@"The content should contain a list of {0} flyers ordered by created date desc", takenumber));
        }

        [When(@"I attempt to retrieve the next (.*) latest flyers using (.*)")]
        public void WhenIAttemptToRetrieveTheNextLatestFlyersUsing(int takenumber, string requestFormat)
        {
            var last = SpecUtil.GetResponseContentAs<ResponseContent<FlyerSummaryContent>>().Data.Flyers.Last();
            SpecUtil.GetRequest(string.Format(requestFormat, takenumber, last.Id));
        }


    }
}
