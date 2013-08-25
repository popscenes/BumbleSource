using System;
using Website.Infrastructure.Domain;

namespace Website.Domain.Location
{
    public static class SuburbEntityInterfaceExtensions
    {
        public static void CopyFieldsFrom(this SuburbEntityInterface suburbTarget, SuburbEntityInterface suburbSource)
        {
            ((SuburbInterface)suburbTarget).CopyFieldsFrom(suburbSource);
            suburbTarget.LocalityExternalId = suburbSource.LocalityExternalId;
            suburbTarget.RegionExternalId = suburbSource.RegionExternalId;
            suburbTarget.ExternalSrc = suburbSource.ExternalSrc;
        }
    }

    public interface SuburbEntityInterface : SuburbInterface, AggregateRootInterface, EntityInterface
    {
        string LocalityExternalId { get; set; }
        string RegionExternalId { get; set; }
        string ExternalSrc { get; set; }
    }

    [Serializable]
    public class Suburb : EntityBase<SuburbEntityInterface>, SuburbEntityInterface
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Locality { get; set; }
        public string LocalityExternalId { get; set; }
        public string Region { get; set; }
        public string RegionCode { get; set; }
        public string RegionExternalId { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string PostCode { get; set; }
        public string ExternalSrc { get; set; }
    }
}