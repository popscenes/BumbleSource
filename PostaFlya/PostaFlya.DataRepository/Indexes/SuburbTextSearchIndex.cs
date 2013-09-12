using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Website.Azure.Common.TableStorage;
using Website.Domain.Location;
using Website.Domain.Query;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Infrastructure.Types;

namespace PostaFlya.DataRepository.Indexes
{
    public class SuburbTextSearchIndex : IndexDefinition<EntityInterface, SuburbEntityInterface>
    {
        public override Expression<Func<QueryChannelInterface, SuburbEntityInterface, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                Expression<Func<QueryChannelInterface, SuburbEntityInterface, IEnumerable<StorageTableKeyInterface>>>
                    indexEntryFactory
                        =
                        (qc, suburb) =>
                        suburb.GetSuburbDescription(true)
                              .ToTermsSearchKeys()
                              .Select((s, i) => new JsonTableEntry(new SearchEntityRecord()
                                  {
                                      Id = suburb.Id,
                                      FriendlyId = suburb.FriendlyId,
                                      TypeOfEntity = suburb.GetType().GetAssemblyQualifiedNameWithoutVer(),
                                      DisplayString = suburb.GetSuburbDescription(true)
                                  })
                                  {

                                      PartitionKey = s + StorageKeyUtil.ToRowKeyWithFieldWidth((int) i, 10).ToStorageKeySection(),
                                      RowKey = suburb.Id.ToStorageKeySection()
                                  });


                return indexEntryFactory;
            }
        }

        public override string IndexName
        {
            get { return "T"; }
        }
    }
}