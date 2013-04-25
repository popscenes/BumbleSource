using System.Collections.Generic;
using Website.Domain.Location;

namespace PostaFlya.Domain.Venue
{
    public interface VenueInterface
    {
        Location Location { get; set; }
        List<VenueInformation> InformationSources { get; set; }
    }
}