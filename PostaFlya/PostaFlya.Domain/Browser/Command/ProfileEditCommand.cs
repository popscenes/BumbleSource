using PostaFlya.Domain.Command;

namespace PostaFlya.Domain.Browser.Command
{
    public class ProfileEditCommand : DomainCommandBase, BrowserIdInterface
    {
        public bool AddressPublic { get; set; }
        public string AvatarImageId { get; set; }
        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public string BrowserId { get; set; }
        public string Handle { get; set; }
        public Location.Location Address { get; set; }
    }
}