using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Extension.Validation;
using Website.Test.Common;

namespace Website.Application.Tests.Extension.Validation
{
    [TestFixture]
    public class ValidationAdaptersTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        class DummyController : Controller
        {
            
        }
        class DummyModel
        {
            public string DummyAttribute { get; set; }
        }

        private ModelClientValidationRule StringLengthValidatorRule()
        {
            var contr = new DummyController();
            ControllerContextMock.FakeControllerContext(Kernel, contr);
            var attribute = new StringLengthAttribute(1000)
                {
                    MinimumLength = 1,
                    ErrorMessage = "{0} is too long."
                };
            var subject = new StringLengthValidator(
                ModelMetadata.FromLambdaExpression(m => m.DummyAttribute
                , new ViewDataDictionary<DummyModel>()), contr.ControllerContext, attribute);
            return subject.GetClientValidationRules().First();            
        }

        [Test]
        public void StringLengthWithMessageValidatorGeneratesCorrectErrorString()
        {
            var rule = StringLengthValidatorRule();
            var errstring = string.Format("{0} is too long.", "DummyAttribute");
            Assert.AreEqual(errstring, rule.ErrorMessage);
        }

        [Test]
        public void StringLengthWithMessageValidatorSetsMinAndMaxProperty()
        {
            var rule = StringLengthValidatorRule();
            Assert.AreEqual(1, rule.ValidationParameters["min"]);
            Assert.AreEqual(1000, rule.ValidationParameters["max"]);
        }


        private ModelClientValidationRule RequiredWithMessageValidatorRule()
        {
            var contr = new DummyController();
            ControllerContextMock.FakeControllerContext(Kernel, contr);
            var attribute = new RequiredAttribute {ErrorMessage = "{0} is required."};
            var subject = new RequiredValidator(
                ModelMetadata.FromLambdaExpression(m => m.DummyAttribute
                , new ViewDataDictionary<DummyModel>()), contr.ControllerContext, attribute);
            return subject.GetClientValidationRules().First();
        }

        [Test]
        public void RequiredWithMessageValidatorGeneratesCorrectErrorString()
        {
            var rule = RequiredWithMessageValidatorRule();
            var errstring = string.Format("{0} is required.", "DummyAttribute");
            Assert.AreEqual(errstring, rule.ErrorMessage);
        }
    }
}
