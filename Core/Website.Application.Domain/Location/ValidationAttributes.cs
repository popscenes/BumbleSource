using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Website.Domain.Location;

namespace Website.Application.Domain.Location
{

    public class ValidLocationAttribute : ValidationAttribute
    { 
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            var loc = value as LocationInterface;
            return loc != null && loc.IsValid();
        }
    }

    public class ValidLocationsAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;
            var locs = value as IEnumerable<LocationInterface>;
            return locs != null && locs.All(loc => loc.IsValid());
        }
    }

    public class ValidLatitudeAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            var loc = value is double ? (double) value : -200;
            return loc >= -90 && loc <= 90;
        }
    }

    public class ValidLongitudeAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            var loc = value is double ? (double)value : -200;
            return loc >= -180 && loc <= 180;
        }
    }
}