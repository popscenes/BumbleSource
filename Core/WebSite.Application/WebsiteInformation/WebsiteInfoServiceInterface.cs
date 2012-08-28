using System;
using System.Collections.Generic;
using System.Linq;

namespace Website.Application.WebsiteInformation
{
    public interface WebsiteInfoServiceInterface
    {

        void RegisterWebsite(string url, WebsiteInfo GetWebsiteInfo);
        String GetBehaivourTags(string url);
        String GetTags(string url);
        string GetWebsiteName(string url);
        WebsiteInfo GetWebsiteInfo(string url);
    }
}