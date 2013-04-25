using System.Collections.Generic;
using PostaFlya.Domain.Venue;
using Website.Domain.Location;

namespace PostaFlya.Domain.Boards
{
    public static class VenueBoardInterfaceExtensions
    {
        public static void CopyFieldsFrom(this VenueBoardInterface target, VenueBoardInterface source)
        {
            target.CopyFieldsFrom((BoardInterface)source);
            target.InformationSources = source.InformationSources != null
                                            ? new List<VenueInformation>(source.InformationSources)
                                            : null;
            target.Location = source.Location != null ? new Location(source.Location) : null;
        }
    }

    public interface VenueBoardInterface : BoardInterface, VenueInterface
    {
        
    }
}