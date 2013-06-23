using Website.Infrastructure.Query;

namespace Website.Application.Domain.TinyUrl.Query
{
    public class FindByTinyUrlQuery : QueryInterface
    {
        public string Url { get; set; }
    }
}