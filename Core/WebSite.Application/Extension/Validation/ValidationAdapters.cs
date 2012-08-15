using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace WebSite.Application.Extension.Validation
{
    public class RequiredWithMessageValidator : DataAnnotationsModelValidator<RequiredWithMessage>
    {

        public RequiredWithMessageValidator(ModelMetadata metaData, ControllerContext context, RequiredWithMessage attribute)
            : base(metaData, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = ErrorMessage,
                ValidationType = "required"//these just need to map to ones provided in validator.unobtrusive.adapters
            };

            return new[] { rule };
        }
    }

    public class StringLengthWithMessageValidator : DataAnnotationsModelValidator<StringLengthWithMessage>
    {

        public StringLengthWithMessageValidator(ModelMetadata metaData, ControllerContext context, StringLengthWithMessage attribute)
            : base(metaData, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage =  ErrorMessage,
                ValidationType = "length",//these just need to map to ones provided in validator.unobtrusive.adapters
                
            };
            rule.ValidationParameters["min"] = Attribute.MinimumLength;
            rule.ValidationParameters["max"] = Attribute.MaximumLength;

            return new[] { rule };
        }
    }

    public class AlphaNumericAndUnderscoreWithMessageValidator : DataAnnotationsModelValidator<AlphaNumericAndUnderscoreWithMessage>
    {

        public AlphaNumericAndUnderscoreWithMessageValidator(ModelMetadata metaData, ControllerContext context, AlphaNumericAndUnderscoreWithMessage attribute)
            : base(metaData, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = ErrorMessage,
                ValidationType = "regex"
            };
            rule.ValidationParameters["pattern"] = Attribute.Regex;

            return new[] { rule };
        }
    }

    public static class ValidationAdapters
    {
        public static void Register()
        {
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(typeof(RequiredWithMessage), typeof(RequiredWithMessageValidator));
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(typeof(StringLengthWithMessage), typeof(StringLengthWithMessageValidator));
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(typeof(AlphaNumericAndUnderscoreWithMessage), typeof(AlphaNumericAndUnderscoreWithMessageValidator));
        }
    }

}
