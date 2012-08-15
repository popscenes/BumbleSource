using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using WebSite.Application.WebsiteInformation;
using WebSite.Azure.Common.TableStorage;

namespace WebSite.Application.Azure.WebsiteInformation
{
    public class WebsiteInfoServiceAzure : AzureRepositoryBase<SimpleExtendableEntity, SimpleExtendableEntity>, WebsiteInfoServiceInterface
    {
        private readonly AzureTableContext _context;

        private const string FIELD_BEHAIVORTAGS = "behaivourtags";
        private const string FIELD_URL = "url";
        private const string FIELD_TAGS = "tags";
        private const string FIELD_WEBSITENAME = "websitename";
        private const string FIELD_FACEBOOKAPPID = "facebookappid";
        private const string FIELD_FACEBOOKSECRET = "facebooksecret";


        public static TableNameAndPartitionProvider<SimpleExtendableEntity> TableNameBinding
            = new TableNameAndPartitionProvider<SimpleExtendableEntity>()
                  {{typeof (SimpleExtendableEntity), 0, "websiteinfo", e => "", e => e.Get<string>("url")}};

        public WebsiteInfoServiceAzure([Named("websiteinfo")]AzureTableContext context)
            : base(context)
        {
            _context = context;
        }

        public void RegisterWebsite(string url, WebsiteInfo GetWebsiteInfo)
        {   
            UpdateEntity(url
                , registrationEntry =>
                {
                    registrationEntry[FIELD_BEHAIVORTAGS] = GetWebsiteInfo.BehaivoirTags.ToString();
                    registrationEntry[FIELD_URL] = url;
                    registrationEntry[FIELD_TAGS] = GetWebsiteInfo.Tags.ToString();
                    registrationEntry[FIELD_WEBSITENAME] = GetWebsiteInfo.WebsiteName;
                    registrationEntry[FIELD_FACEBOOKAPPID] = GetWebsiteInfo.FacebookAppID;
                    registrationEntry[FIELD_FACEBOOKSECRET] = GetWebsiteInfo.FacebookAppSecret;


                    registrationEntry.RowKey = url;
                    registrationEntry.PartitionKey = "";
                });

            SaveChanges();
        }

        public String GetBehaivourTags(string url)
        {
            var websites = _context.PerformQuery<SimpleExtendableEntity>();
            var websiteTags = websites
                .Where(_ => _.RowKey == url)
                .Select(_ => _.Get<string>(FIELD_BEHAIVORTAGS)).FirstOrDefault();

            return websiteTags;
        }

        public String GetTags(string url)
        {
            var websites = _context.PerformQuery<SimpleExtendableEntity>();
            var tags = websites
                .Where(_ => _.RowKey == url)
                .Select(_ => _.Get<string>(FIELD_TAGS)).FirstOrDefault();

            return tags;

        }

        public string GetWebsiteName(string url)
        {
            var websites = _context.PerformQuery<SimpleExtendableEntity>();
            var websitename = websites
                .Where(_ => _.RowKey == url)
                .Select(_ => _.Get<string>(FIELD_WEBSITENAME)).FirstOrDefault();

            return websitename;
        }

        protected override SimpleExtendableEntity GetEntityForUpdate(string url)
        {
            var registrationEntries = _context.PerformQuery<SimpleExtendableEntity>();
            return registrationEntries.SingleOrDefault(e => e.Get<string>(FIELD_URL) == url) ??
                                    new SimpleExtendableEntity();
        }

        public WebsiteInfo GetWebsiteInfo(string url)
        {
            var websites = _context.PerformQuery<SimpleExtendableEntity>();

            var websiteInfo = websites
                .Where(_ => _.RowKey == url).Select(_ => new WebsiteInfo()
                {
                    Tags = _.Get<string>(FIELD_TAGS),
                    BehaivoirTags = _.Get<string>(FIELD_BEHAIVORTAGS),
                    FacebookAppID = _.Get<string>(FIELD_FACEBOOKAPPID),
                    FacebookAppSecret = _.Get<string>(FIELD_FACEBOOKSECRET),
                    WebsiteName = _.Get<string>(FIELD_WEBSITENAME)

                }
            ).FirstOrDefault();

            return websiteInfo;
        }

        protected override SimpleExtendableEntity GetStorageForEntity(SimpleExtendableEntity entity)
        {
            return entity;
        }
    }
}
