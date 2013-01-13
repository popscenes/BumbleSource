using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    public class ProfileEditCommand : DefaultCommandBase, BrowserIdInterface
    {
        public string AvatarImageId { get; set; }
        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public string BrowserId { get; set; }
        public string Handle { get; set; }
        public Location.Location Address { get; set; }
        public string WebSite { get; set; }
    }
}