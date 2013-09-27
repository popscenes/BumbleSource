using Website.Infrastructure.Query;

namespace Website.Domain.Location.Query
{
    public class PopulateSuburbQuery :  QueryInterface<Suburb>
    {
        public Suburb Suburb { get; set; }
    }
}