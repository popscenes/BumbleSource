using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Indexes
{
    public class BrowserIdForAggregateMemberIndex<EntityIndexType> :
    BrowserIdForAggregateMemberIndex<EntityIndexType, EntityIndexType>
    where EntityIndexType : class, AggregateInterface, BrowserIdInterface
    { }

    public class BrowserIdForAggregateMemberIndex<EntityQueryType, EntityIndexType> :
        IndexDefinition<EntityQueryType, EntityIndexType>
        where EntityIndexType : class, EntityQueryType, AggregateInterface
        where EntityQueryType : BrowserIdInterface, EntityIdInterface
    {
        public override Expression<Func<QueryChannelInterface, EntityIndexType, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                Expression<Func<QueryChannelInterface, EntityIndexType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory =
                    (qc, root) => new List<StorageTableKeyInterface>()
                        {
                            new StorageTableKey()
                                {
                                    PartitionKey = root.BrowserId.ToStorageKeySection(),
                                    RowKey = root.AggregateId.ToStorageKeySection() + root.Id.ToStorageKeySection()
                                }
                        };
                return indexEntryFactory;
            }
        }

        public override string IndexName
        {
            get { return "BrowserId"; }
        }
    }
}