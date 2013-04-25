using System.Collections.Generic;
using PostaFlya.Domain.Venue;
using Website.Domain.Location;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards
{
    public class VenueBoard : Board, VenueBoardInterface
    {
        public Location Location { get; set; }
        public List<VenueInformation> InformationSources { get; set; }
    }
}