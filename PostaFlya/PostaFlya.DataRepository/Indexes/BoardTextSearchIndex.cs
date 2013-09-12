using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PostaFlya.Domain.Boards;
using Website.Azure.Common.TableStorage;
using Website.Domain.Query;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Infrastructure.Types;

namespace PostaFlya.DataRepository.Indexes
{
    public class BoardTextSearchIndex : IndexDefinition<EntityInterface, BoardInterface>
    {
        public override Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>>
                    indexEntryFactory
                        =
                        (qc, board) =>
                        board.Name
                             .ToTermsSearchKeys()
                             .Select((s, i) => new JsonTableEntry(new SearchEntityRecord()
                                 {
                                     Id = board.Id,
                                     FriendlyId = board.FriendlyId,
                                     TypeOfEntity = board.GetType().GetAssemblyQualifiedNameWithoutVer(),
                                     DisplayString = board.Name
                                 })
                                 {
                                     PartitionKey = s + ((int) i).ToRowKeyWithFieldWidth(10).ToStorageKeySection(),
                                     RowKey = board.Id.ToStorageKeySection()
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