using Website.Domain.Browser;

namespace Website.Application.Domain.Browser
{
    public interface BrowserInformationInterface
    {
        BrowserInterface Browser { get; set; }
        string IpAddress { get; }
        string UserAgent { get; }
        string TrackingId { get; set; }
    }
}