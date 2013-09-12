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
        public const int DefaultNearByIndex = 40;
        public override Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {

                Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory =
                    (qc, board) => board.Venue() == null ? new List<JsonTableEntry>() :

                                       qc.Query(new FindSuburbsWithinDistanceOfGeoCoordsQuery
                                           {
                                               Geo = board.Venue().Address.AsGeoCoords(),
                                               Kilometers = DefaultNearByIndex
                                           }, new List<Suburb>())
                                         .Select(suburb =>
                                                 new JsonTableEntry(board.Venue().Address.AsGeoCoords())
                                                     {
                                                         PartitionKey = StorageKeyUtil.ToStorageKeySection(suburb.Id),
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