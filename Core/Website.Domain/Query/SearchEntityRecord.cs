using Website.Infrastructure.Domain;

namespace Website.Domain.Query
{
    public class SearchEntityRecord : EntityIdInterface
    {
        public string Id { get; set; }
        public string FriendlyId { get; set; }
        public string TypeOfEntity { get; set; }
        public string DisplayString { get; set; }
        public string[] SearchTerms { get; set; }
    }
}