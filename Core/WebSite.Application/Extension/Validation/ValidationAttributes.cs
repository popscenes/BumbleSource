using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using DataAnnotationsExtensions;

namespace WebSite.Application.Extension.Validation
{
    //These are in part to enable easier localization in the future without
    //massive attributes all over properties, however also
    //for custom attributes
    public static class ErrorStrings
    {
        //TODO use localised string resource
        public static string StringTooLarge
        {
            get { return "{0} is too long."; }
        }

        public static string Required
        {
            get { return "{0} is required."; }
        }

        public static string InvalidEmailAddress
        {
            get { return "{0} is not a valid email address."; }
        }

        public static string InvalidCharacters
        {
            get { return "{0} can contain only letters, digits and underscores"; }
        }
    }

    public class EmailAddressWithMessage : EmailAttribute//can prolly replace WHEN .NET4.5
    {
        public EmailAddressWithMessage()
        {
            ErrorMessageResourceType = typeof(ErrorStrings);
            ErrorMessageResourceName = "InvalidEmailAddress";
        }
    }

    public class StringLengthWithMessage : StringLengthAttribute
    {
        public StringLengthWithMessage(int maximumLength) : base(maximumLength)
        {
            ErrorMessageResourceType = typeof(ErrorStrings);
            ErrorMessageResourceName = "StringTooLarge";
        }
    }

    public class RequiredWithMessage : RequiredAttribute
    {
        public RequiredWithMessage()
        {
            ErrorMessageResourceType = typeof(ErrorStrings);
            ErrorMessageResourceName = "Required";
        }
    }

    public class RangeWithMessage : RangeAttribute
    {
        public RangeWithMessage(double minimum, double maximum)
            : base(minimum, maximum)
        {
            ErrorMessageResourceType = typeof(ErrorStrings);
            ErrorMessageResourceName = "InvalidRange";
        }
    }

    public class AlphaNumericAndUnderscoreWithMessage : AlphaNumericAndUnderscoreAttribute
    {
        public AlphaNumericAndUnderscoreWithMessage()
        {
            ErrorMessageResourceType = typeof(ErrorStrings);
            ErrorMessageResourceName = "InvalidCharacters";
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AlphaNumericAndUnderscoreAttribute : DataTypeAttribute
    {
        private static readonly Regex _regex = new Regex("^[a-zA-Z0-9_]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string Regex
        {
            get
            {
                return _regex.ToString();
            }
        }

        static AlphaNumericAndUnderscoreAttribute()
        {
        }

        public AlphaNumericAndUnderscoreAttribute()
            : base(DataType.Text)
        {
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;
            var input = value as string;
            if (input != null)
                return _regex.Match(input).Length > 0;
            return false;
        }
    }

}