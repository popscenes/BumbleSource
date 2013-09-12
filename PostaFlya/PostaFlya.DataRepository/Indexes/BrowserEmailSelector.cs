using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Website.Azure.Common.TableStorage;
using Website.Domain.Browser;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Indexes
{
    public class BrowserEmailSelector : IndexDefinition<BrowserInterface, BrowserInterface>
    {
        public override Expression<Func<QueryChannelInterface, BrowserInterface, IEnumerable<StorageTableKeyInterface>>> Definition
        {
            get
            {
                Expression<Func<QueryChannelInterface, BrowserInterface, IEnumerable<StorageTableKeyInterface>>>
                    indexEntryFactory
                        = (qc, browser) =>

                          new List<StorageTableKeyInterface>()
                              {
                                  new StorageTableKey()
                                      {
                                          PartitionKey = browser.EmailAddress.ToStorageKeySection(),
                                          RowKey = browser.Id.ToStorageKeySection()
                                      }
                              };

                return indexEntryFactory;
            }
        }

        public override string IndexName
        {
            get { return "BrowserEmail"; }
        }
    }
}