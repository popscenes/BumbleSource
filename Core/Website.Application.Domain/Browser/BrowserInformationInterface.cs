using Website.Domain.Browser;

namespace Website.Application.Domain.Browser
{
    public static class BrowserInformationExtensions
    {
        public static bool IsOrCanAccessBrowser(this BrowserInformationInterface browserInformation, string browserid, string handle)
        {
            return browserInformation.Browser.HasRole(Role.Admin) ||
                    browserInformation.Browser.Id == browserid ||
                   (string.IsNullOrWhiteSpace(browserid) && browserInformation.Browser.FriendlyId == handle);
        }
    }
    public interface BrowserInformationInterface
    {
        BrowserInterface Browser { get; set; }
        string IpAddress { get; }
        string UserAgent { get; }
        string TrackingId { get; set; }
    }
}