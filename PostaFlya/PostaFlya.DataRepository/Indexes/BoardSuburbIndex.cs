using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PostaFlya.Domain.Boards;
using Website.Azure.Common.TableStorage;
using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Indexes
{
    public class BoardSuburbIndex : IndexDefinition<BoardInterface, BoardInterface>
    {
        public override Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {

                Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory =
                    (qc, board) => board.Venue() == null ? new List<JsonTableEntry>() :

                                       qc.Query(new FindSuburbsWithinDistanceOfGeoCoordsQuery
                                           {
                                               Geo = board.Venue().Address.AsGeoCoords(),
                                               Kilometers = Defaults.DefaultNearByIndex
                                           }, new List<Suburb>())
                                         .Select(suburb =>
                                                 new JsonTableEntry(
                                                     new GeoPoints(board.Venue().Address, suburb, Defaults.Distance))
                                                     {
                                                         PartitionKey = suburb.Id.ToStorageKeySection(),
                                                         RowKey = board.Id.ToStorageKeySection()
                                                     });

                return indexEntryFactory;       

            }
        }

        public override string IndexName
        {
            get { return "BS"; }
        }
    }
}