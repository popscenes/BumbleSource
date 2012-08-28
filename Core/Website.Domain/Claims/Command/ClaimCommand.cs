using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Domain.Claims.Command
{
    public class ClaimCommand : DefaultCommandBase
    {
        public string Context { get; set; }
        public EntityInterface ClaimEntity { get; set; }
        public string BrowserId { get; set; }
    }
}