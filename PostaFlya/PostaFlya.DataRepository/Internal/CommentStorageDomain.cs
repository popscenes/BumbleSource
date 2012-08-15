using System;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Comments;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.DataRepository.Internal
{
    internal class CommentTableEntry : ExtendableTableEntry
        , StorageTableEntryInterface<CommentInterface>
    {
        public void Update(CommentInterface source)
        {
            this["Id"] = source.Id;
            this["EntityId"] = source.EntityId;
            this["CommentContent"] = source.CommentContent;
            this["BrowserId"] = source.BrowserId;
            this["CommentTime"] = source.CommentTime;
        }

        public void UpdateEntity(CommentInterface target)
        {
            target.Id = Get<string>("Id");
            target.EntityId = Get<string>("EntityId");
            target.CommentContent = Get<string>("CommentContent");
            target.BrowserId = Get<string>("BrowserId");
            target.CommentTime = Get<DateTime>("CommentTime");
        }
    }

    internal class CommentStorageDomain
        : StorageDomainEntityBase<CommentStorageDomain, CommentTableEntry, CommentInterface, Comment>
    {
        public static TableNameAndPartitionProvider<CommentInterface>
            TableNamesAndPartition = new TableNameAndPartitionProvider<CommentInterface>()
                                         {
                                             {typeof(CommentTableEntry), IdPartition, "comment", i => i.Id, GetIdKey},
                                             {typeof(CommentTableEntry), AggregateIdPartition, "comment", i => i.EntityId, GetIdKey},        
                                         };

        public CommentStorageDomain(TableNameAndPartitionProviderInterface<CommentInterface> tableNameProvider
                                    , AzureTableContext tableContext) : base(tableNameProvider, tableContext)
        {
        }

        protected internal CommentStorageDomain(CommentInterface source, AzureTableContext tableContext)
            : this(TableNamesAndPartition, tableContext)
        {
            this.DomainEntity.CopyFieldsFrom(source);
            ClonedTable.CreateDefaultEntries();
        }

        public CommentStorageDomain()
            : base(TableNamesAndPartition)
        {

        }

        public static string GetIdKey(CommentInterface comment)
        {
            return comment.CommentTime.Ticks.ToString("D20") + comment.Id;
        }
    }
}