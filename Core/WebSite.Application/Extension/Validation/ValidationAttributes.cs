using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using DataAnnotationsExtensions;
using Website.Application.Properties;
using UrlAttribute = System.ComponentModel.DataAnnotations.UrlAttribute;

namespace Website.Application.Extension.Validation
{
 
    public class StringLengthWithMessage : StringLengthAttribute
    {
        public StringLengthWithMessage(int maximumLength) : base(maximumLength)
        {
            ErrorMessageResourceType = typeof(Resources);
            ErrorMessageResourceName = "StringTooLarge";
        }
    }

    public class RangeWithMessage : RangeAttribute
    {
        public RangeWithMessage(double minimum, double maximum)
            : base(minimum, maximum)
        {
            ErrorMessageResourceType = typeof(Resources);
            ErrorMessageResourceName = "InvalidRange";
        }
    }

    public class AlphaNumericAndUnderscoreWithMessage : AlphaNumericAndUnderscoreAttribute
    {
        public AlphaNumericAndUnderscoreWithMessage()
        {
            ErrorMessageResourceType = typeof(Resources);
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

    public class ConvertableToGuidAttributeWithMessage : ConvertableToGuidAttribute
    {
        public ConvertableToGuidAttributeWithMessage()
        {
            ErrorMessageResourceType = typeof(Resources);
            ErrorMessageResourceName = "InvalidGuid";
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ConvertableToGuidAttribute : DataTypeAttribute
    {
        public ConvertableToGuidAttribute()
            : base(DataType.Text)
        {
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            if (value is Guid)
                return true;
            
            var input = value as string;
            if (input != null)
            {
                Guid output;
                return Guid.TryParse(input, out output);
            }
            return false;
        }
    }

    public class CollectionCountWithMessageAttribute : CollectionCountAttribute
    {
        public CollectionCountWithMessageAttribute(int minimum, int maximum = -1)
            : base(minimum, maximum)
        {
            ErrorMessageResourceType = typeof(Resources);
            ErrorMessageResourceName = "InvalidCount";
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class CollectionCountAttribute : ValidationAttribute
    {

        public int MaximumLength { get; set; }

        public int MinimumLength { get; set; }


        public CollectionCountAttribute(int minimum, int maximum = -1)
        {
            ErrorMessageResourceType = typeof(Resources);
            ErrorMessageResourceName = "InvalidCount";
            this.MinimumLength = minimum;
            this.MaximumLength = maximum;
        }

        public override bool IsValid(object value)
        {
            var collection = value as ICollection;
            if (collection != null)
            {
                return collection.Count > MinimumLength && (MaximumLength < 0 || collection.Count < MaximumLength);
            }

            return false;
        }        
    }

}