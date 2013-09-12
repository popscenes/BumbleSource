using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PostaFlya.Domain.Boards;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Indexes
{
    public class BoardAdminEmailIndex : IndexDefinition<BoardInterface, BoardInterface>
    {
        public override Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                Expression<Func<QueryChannelInterface, BoardInterface, IEnumerable<StorageTableKeyInterface>>>
                    indexEntryFactory
                        = (qc, board) =>
                          board.AdminEmailAddresses == null
                              ? new List<StorageTableKey>()
                                  {
                                      new StorageTableKey() {RowKey = board.Id.ToStorageKeySection()}
                                  }
                              : board.AdminEmailAddresses.Select(
                                  adimEmail =>
                                  new StorageTableKey()
                                      {
                                          PartitionKey = StorageKeyUtil.ToStorageKeySection(adimEmail),
                                          RowKey = board.Id.ToStorageKeySection()
                                      });

                return indexEntryFactory;
            }
        }

        public override string IndexName
        {
            get { return "BoardAdminEmail"; }
        }
    }
}