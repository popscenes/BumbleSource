using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebSite.Application.Extension.Validation;
using PostaFlya.Domain.Location;

namespace PostaFlya.Application.Domain.Location
{
    //These are in part to enable easier localization in the future without
    //massive attributes all over properties, however also
    //for custom attributes such as the Location one below

    public static class ErrorStrings
    {
        //TODO use localised string resource

        public static string InvalidLocation//TODO test format string is support in required att
        {
            get { return "{0} must be a valid location."; }
        }
    }


    public class ValidLocationAttribute : ValidationAttribute
    {
        public ValidLocationAttribute()
        {
            ErrorMessageResourceType = typeof (ErrorStrings);
            ErrorMessageResourceName = "InvalidLocation";
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            var loc = value as LocationInterface;
            return loc != null && (value as LocationInterface).IsValid();
        }
    }

    public class ValidLocationsAttribute : ValidationAttribute
    {
        public ValidLocationsAttribute()
        {
            ErrorMessageResourceType = typeof(ErrorStrings);
            ErrorMessageResourceName = "InvalidLocation";
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;
            var locs = value as IEnumerable<LocationInterface>;
            return locs != null && locs.All(loc => loc.IsValid());
        }
    }
}