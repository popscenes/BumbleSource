using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PostaFlya.Domain.Flier;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Indexes
{
    public class FlyerBoardsIndex : IndexDefinition<FlierInterface, FlierInterface>
    {
        public override Expression<Func<QueryChannelInterface, FlierInterface, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                Expression<Func<QueryChannelInterface, FlierInterface, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory =
                    (qc, flyer) => flyer
                                       .Boards
                                       .Select(bf => bf.BoardId)
                                       .Distinct()
                                       .SelectMany(b
                                                   => flyer.EventDates.Distinct().Select(e =>
                                                                                         new StorageTableKey()
                                                                                             {
                                                                                                 PartitionKey = b.ToStorageKeySection() + e.GetTimestampAscending().ToStorageKeySection(),
                                                                                                 RowKey = flyer.Id.ToStorageKeySection()
                                                                                             }
                                                          ));

                return indexEntryFactory;
            }
        }

        public override string IndexName
        {
            get { return "FB"; }
        }
    }
}