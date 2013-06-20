using System.Linq;
using System.Net;
using System.Net.Http;
using NUnit.Framework;
using Popscenes.Specification.Util;
using PostaFlya.Areas.MobileApi.Infrastructure.Model;
using TechTalk.SpecFlow;

namespace Popscenes.Specification
{
    [Binding]
    public class CommonSteps : Steps
    {
        // For additional details on SpecFlow step definitions see http://go.specflow.org/doc-stepdef

        [Then(@"I should receive a http response with a status of (.*)")]
        public void ThenIShouldReceiveAHttpResponseWithAStatusOf(HttpStatusCode httpStatus)
        {
            Assert.That(SpecUtil.ResponseMessage.StatusCode, Is.EqualTo(httpStatus));
        }

        [Then(@"The content should have a response status of (.*)")]
        public void ThenTheContentShouldHaveAResponseStatusOf(ResponseContent.StatusEnum statusEnum)
        {
            var msg = SpecUtil.GetResponseContentAs<ResponseContent>();
            Assert.That(msg, Is.Not.Null);
            Assert.That(msg.Code, Is.EqualTo((int)statusEnum));
        }

        [Given(@"I perform a get request for the path (.*)")]
        [When(@"I perform a get request for the path (.*)")]
        public void WhenIPerformAGetRequestForThePath(string path)
        {
            SpecUtil.GetRequest(path);
        }

        [When(@"I perform a post request for the path (.*)")]
        public void WhenIPerformAPostRequestForThePath(string path)
        {
            SpecUtil.Request(path, SpecUtil.RequestObject, HttpMethod.Post);
        }

        [Then(@"The content should have a message stating (.*)")]
        public void ThenTheContentShouldHaveAMessageStating(string message)
        {
            var res = SpecUtil.GetResponseContentAs<ResponseContent>();
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Messages.Length, Is.GreaterThan(0));
            Assert.True(res.Messages.Any(msg => msg.ToLowerInvariant().Equals(message.ToLowerInvariant())));
        }


        [Then(@"The validation error message for the property (.*) should be (.*)")]
        public void ThenTheValidationErrorMessageForThePropertyShouldBe(string validationProperty, string message)
        {
            var res = SpecUtil.GetResponseContentAs<ResponseContent<ValidationErrorModel>>();
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Data.Errors, Is.Not.Null);
            Assert.That(res.Data.Errors.Count, Is.GreaterThan(0));
            var field = res.Data.Errors.FirstOrDefault(entry => entry.Property == validationProperty);
            Assert.That(field, Is.Not.Null);
            Assert.That(field.Message, Is.EqualTo(message));
        }

    }
}
