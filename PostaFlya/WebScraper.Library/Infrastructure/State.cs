using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebScraper.Library.Properties;

namespace WebScraper.Library.Infrastructure
{
    public class SiteState
    {
        public string SiteName { get; set; }
        public DateTime LastGigDateUpdated { get; set; }
    }

    public static class State
    {
        public static Indexer Site { get; set; }



        private static Dictionary<string, SiteState> siteStates = null;

        static State()
        {
            Site = new Indexer();
        }

        public class Indexer
        {
            public SiteState this[string sitename]
            {
                get { return GetState(sitename); }
                set { SetState(sitename, value); }
            }
        }

        private static void SetState(string siteName, SiteState state)
        {
            if (siteStates == null)
            {
                try
                {
                    siteStates =
                        JsonConvert.DeserializeObject<Dictionary<string, SiteState>>(Settings.Default.JsonDataStore);
                }
                finally
                {
                    if (siteStates == null)
                        siteStates = new Dictionary<string, SiteState>();
                }

            }

            siteStates[siteName] = state;
            Settings.Default.JsonDataStore = JsonConvert.SerializeObject(siteStates);
        }
 
        private static SiteState GetState(string siteName)
        {
            if (siteStates == null)
            {
                try
                {
                    siteStates =
                        JsonConvert.DeserializeObject<Dictionary<string, SiteState>>(Settings.Default.JsonDataStore);
                }
                finally
                {
                    if (siteStates == null)
                        siteStates = new Dictionary<string, SiteState>();
                }

            }

            SiteState ret;
            siteStates.TryGetValue(siteName, out ret);
            return ret;
        }

    
    }
}
