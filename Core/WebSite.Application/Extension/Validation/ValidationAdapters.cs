using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Website.Application.Extension.Validation
{
    public class RequiredValidator : DataAnnotationsModelValidator<RequiredAttribute>
    {

        public RequiredValidator(ModelMetadata metaData, ControllerContext context, RequiredAttribute attribute)
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

    public class StringLengthValidator : DataAnnotationsModelValidator<StringLengthAttribute>
    {

        public StringLengthValidator(ModelMetadata metaData, ControllerContext context, StringLengthAttribute attribute)
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

    public class AlphaNumericAndHiphenValidator : DataAnnotationsModelValidator<AlphaNumericAndHiphenAttribute>
    {

        public AlphaNumericAndHiphenValidator(ModelMetadata metaData, ControllerContext context, AlphaNumericAndHiphenAttribute attribute)
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
                .RegisterAdapter(typeof(RequiredAttribute), typeof(RequiredValidator));
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(typeof(StringLengthAttribute), typeof(StringLengthValidator));
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(typeof(AlphaNumericAndHiphenAttribute), typeof(AlphaNumericAndHiphenValidator));
        }
    }

}
