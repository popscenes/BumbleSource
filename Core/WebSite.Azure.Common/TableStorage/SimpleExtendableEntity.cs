using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Domain;

namespace WebSite.Azure.Common.TableStorage
{
    /// <summary>
    /// Just for cases where you just need a simple entity, ie just maps to one table entry and there are no partition clones etc
    /// 
    /// </summary>
    public class SimpleExtendableEntity : ExtendableTableEntry, StorageDomainEntityInterface<SimpleExtendableEntity>, EntityIdInterface
    {
        public IEnumerable<StorageTableEntryInterface> GetTableEntries()
        {
            return new List<StorageTableEntryInterface>() { this };
        }

        public SimpleExtendableEntity DomainEntity
        {
            get { return this; }
        }

        public string Id
        {
            get { return Get<string>("Id"); }
            set { this["Id", typeof (string)] = value; }
        }
    }
}
