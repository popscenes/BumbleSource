using System;
using System.Linq;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Likes;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.DataRepository.Internal
{
    internal class LikeTableEntry : ExtendableTableEntry
        , StorageTableEntryInterface<LikeInterface>
    {
        public string EntityTypeTag { get { return Get<string>("EntityTypeTag"); } set { this["EntityTypeTag"] = value; } }
        public void Update(LikeInterface source)
        {
            this["EntityId"] = source.EntityId;
            this["LikeContent", typeof(string)] = source.LikeContent;
            this["BrowserId"] = source.BrowserId;
            this["ILike"] = source.ILike;
            this["LikeTime"] = source.LikeTime;
            this["EntityTypeTag"] = source.EntityTypeTag;
        }

        public void UpdateEntity(LikeInterface target)
        {

            target.EntityId = Get<string>("EntityId");
            target.LikeContent = Get<string>("LikeContent");
            target.BrowserId = Get<string>("BrowserId");
            target.ILike = Get<bool>("ILike");
            target.LikeTime = Get<DateTime>("LikeTime");
            target.EntityTypeTag = Get<string>("EntityTypeTag");
        }
    }

    internal class LikeStorageDomain
        : StorageDomainEntityBase<LikeStorageDomain, LikeTableEntry, LikeInterface, Like>
    {
        public const int BrowserIdPartition = 1;
        public static TableNameAndPartitionProvider<LikeInterface>
            TableNamesAndPartition = new TableNameAndPartitionProvider<LikeInterface>()
                                         {
                                             {typeof(LikeTableEntry), IdPartition, "like", GetIdPartitionKey, i => i.BrowserId},
                                             {typeof(LikeTableEntry), AggregateIdPartition, "like", i => i.EntityId, GetAggregateRowKey},
                                             {typeof(LikeTableEntry), BrowserIdPartition, "like", i => i.BrowserId, GetBrowserRowKey},
                                         };

        public LikeStorageDomain(TableNameAndPartitionProviderInterface<LikeInterface> tableNameProvider
                                    , AzureTableContext tableContext) : base(tableNameProvider, tableContext)
        {
        }

        protected internal LikeStorageDomain(LikeInterface source, AzureTableContext tableContext)
            : this(TableNamesAndPartition, tableContext)
        {

            this.DomainEntity.CopyFieldsFrom(source);
            ClonedTable.CreateDefaultEntries();
        }

        public LikeStorageDomain()
            : base(TableNamesAndPartition)
        {

        }

        public static string GetAggregateRowKey(LikeInterface like)//order by first like
        {
            return like.BrowserId;
        }

        public static string GetBrowserRowKey(LikeInterface like)//order by most recent
        {
            return like.EntityId;
        }

        public static string GetIdPartitionKey(LikeInterface like)
        {
            return like.EntityId + like.BrowserId;
        }

        public static IQueryable<LikeInterface> FindByBrowserAndEntityTypeTag(string bropwserId, string entityTypeTag, AzureTableContext tableContext)
        {
            return FindEntities(tableContext, l => l.PartitionKey == bropwserId && l.EntityTypeTag == entityTypeTag, BrowserIdPartition);
        }
    }
}