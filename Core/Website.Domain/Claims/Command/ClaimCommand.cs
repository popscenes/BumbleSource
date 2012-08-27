using WebSite.Infrastructure.Domain;
using Website.Domain.Command;

namespace Website.Domain.Claims.Command
{
    public class ClaimCommand : DomainCommandBase
    {
        public string Comment { get; set; }
        public EntityInterface ClaimEntity { get; set; }
        public string BrowserId { get; set; }
    }
}