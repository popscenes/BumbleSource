using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using DataAnnotationsExtensions;
using Website.Application.Properties;
using UrlAttribute = System.ComponentModel.DataAnnotations.UrlAttribute;

namespace Website.Application.Extension.Validation
{
 
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AlphaNumericAndHiphenAttribute : DataTypeAttribute
    {
        private static readonly Regex _regex = new Regex("^[-a-zA-Z0-9]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string Regex
        {
            get
            {
                return _regex.ToString();
            }
        }

        static AlphaNumericAndHiphenAttribute()
        {
        }

        public AlphaNumericAndHiphenAttribute()
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

        public override string FormatErrorMessage(string name)
        {
            return string.Format((IFormatProvider)CultureInfo.CurrentCulture, this.ErrorMessageString, (object)name, this.MinimumLength, this.MaximumLength);
        }
    }

}