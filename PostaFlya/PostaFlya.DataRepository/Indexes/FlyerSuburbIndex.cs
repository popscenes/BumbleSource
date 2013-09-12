using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.TableStorage;
using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Indexes
{
    public class FlyerSuburbIndex : IndexDefinition<FlierInterface, FlierInterface>
    {
        private const int DefaultNearByIndex = 40;

        public override Expression<Func<QueryChannelInterface, FlierInterface, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                Expression<Func<QueryChannelInterface, FlierInterface, IEnumerable<StorageTableKeyInterface>>>
                    indexEntryFactory =
                        (qc, flyer) =>
                        qc.Query(new FindSuburbsWithinDistanceOfGeoCoordsQuery
                            {
                                Geo = flyer.GetVenueForFlier(qc).Address.AsGeoCoords(),
                                Kilometers = DefaultNearByIndex
                            }, new List<Suburb>())
                          .Select(s => new {s, v = flyer.GetVenueForFlier(qc).Address.AsGeoCoords()})
                          .SelectMany(sv =>
                                      flyer.EventDates.Distinct().Select(e =>
                                                                         new JsonTableEntry(sv.v)
                                                                             {
                                                                                 PartitionKey =
                                                                                     sv.s.Id.ToStorageKeySection() +
                                                                                     e.GetTimestampAscending()
                                                                                      .ToStorageKeySection(),
                                                                                 RowKey = flyer.Id.ToStorageKeySection()
                                                                             }));

                return indexEntryFactory;
            }
        }

        public override string IndexName
        {
            get { return "FS"; }
        }
    }
}