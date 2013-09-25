using Website.Infrastructure.Domain;

namespace Dark
{


    public interface AggregateRootMemberTableInterface : AggregateRootInterface
    {
        long RowId { get; set; }
        long ShardId { get; set; }
    }

    public class AggregateRootMemberTable : AggregateRootMemberTableInterface
    {
        public string Id { get; set; }
        public string FriendlyId { get; set; }
        public long RowId { get; set; }
        public long ShardId { get; set; }
    }

    public class AggregateRootTable : AggregateRootMemberTable
    {
        public string Json { get; set; }
        public long JsonHash { get; set; }
        public string ClrType { get; set; }
    }
}