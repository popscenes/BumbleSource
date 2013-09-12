using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Indexes
{
    public class BrowserIdIndex<EntityIndexType> :
        IndexDefinition<EntityIndexType, EntityIndexType>
        where EntityIndexType : class, AggregateRootInterface, BrowserIdInterface, EntityIdInterface
    {

        public override Expression<Func<QueryChannelInterface, EntityIndexType, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                Expression<Func<QueryChannelInterface, EntityIndexType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory = (qc, root) => new List<StorageTableKeyInterface>()
                    {
                        new StorageTableKey()
                            {
                                PartitionKey = root.BrowserId.ToStorageKeySection(),
                                RowKey = root.Id.ToStorageKeySection()
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