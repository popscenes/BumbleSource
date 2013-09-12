using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Website.Azure.Common.TableStorage;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Indexes
{
    public class SuburbGeoSearchIndex : IndexDefinition<SuburbEntityInterface, SuburbEntityInterface>
    {
        public static string ToGeoLatSearchKey( double lat)
        {
            return Math.Round(lat + 90, 3).ToString("000.000").ToStorageKeySection();
        }

        public static string ToGeoLongSearchKey( double @long)
        {
            return Math.Round(@long + 180, 3).ToString("000.000").ToStorageKeySection();
        }

        public override Expression<Func<QueryChannelInterface, SuburbEntityInterface, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                return
                    (qc, suburb) => new List<StorageTableKeyInterface>()
                        {
                            new JsonTableEntry(new GeoCoords()
                                {
                                    Latitude = suburb.Latitude,
                                    Longitude = suburb.Longitude
                                })
                                {
                                    PartitionKey = ToGeoLongSearchKey(suburb.Longitude),
                                    RowKey = ToGeoLatSearchKey(suburb.Latitude) + suburb.Id.ToStorageKeySection()
                                }
                        };
            }
        }

        public override string IndexName
        {
            get { return "G"; }
        }
    }
}