using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Website.Azure.Common.TableStorage;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Indexes
{
    public class TinyUrlIndex<EntityIndexType> : 
        IndexDefinition<EntityIndexType, EntityIndexType> 
        where EntityIndexType : class, EntityInterface, TinyUrlInterface
    {
        public override Expression<Func<QueryChannelInterface, EntityIndexType, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                Expression<Func<QueryChannelInterface, EntityIndexType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory = (qc, root) => new List<StorageTableKeyInterface>()
                    {
                        new JsonTableEntry(new EntityKeyWithTinyUrl()
                            {
                                Id = root.Id, 
                                TinyUrl = root.TinyUrl, 
                                PrimaryInterface = root.PrimaryInterface, 
                                FriendlyId = root.FriendlyId
                            })
                            {
                           
                                PartitionKey = root.TinyUrl.ToStorageKeySection(),
                                RowKey = root.Id.ToStorageKeySection()
                            }
                    };
                return indexEntryFactory;
            }
        }

        public override string IndexName
        {
            get { return "TinyUrl"; }
        }
    }
}