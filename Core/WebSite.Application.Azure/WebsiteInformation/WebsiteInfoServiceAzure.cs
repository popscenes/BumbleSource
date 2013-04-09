using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ninject;
using Website.Infrastructure.Domain;
using Website.Application.WebsiteInformation;
using Website.Azure.Common.TableStorage;

namespace Website.Application.Azure.WebsiteInformation
{
    public class WebsiteInfoEntity : SimpleExtendableEntity
    {
        public bool IsDefault { get; set; }
    }
    public class WebsiteInfoServiceAzure : RepositoryBase<WebsiteInfoEntity>, WebsiteInfoServiceInterface
    {

        private const string FIELD_BEHAIVORTAGS = "behaivourtags";
        private const string FIELD_URL = "url";
        private const string FIELD_TAGS = "tags";
        private const string FIELD_WEBSITENAME = "websitename";
        private const string FIELD_FACEBOOKAPPID = "facebookappid";
        private const string FIELD_FACEBOOKSECRET = "facebooksecret";

        private const string FIELD_PAYPALID = "paypalId";
        private const string FIELD_PAYPALPW = "paypalPw";
        private const string FIELD_PAYPALSIG = "paypalSig";


//        public static TableNameAndPartitionProvider<SimpleExtendableEntity> TableNameBinding
//            = new TableNameAndPartitionProvider<SimpleExtendableEntity>()
//                  {{typeof (SimpleExtendableEntity), 0, "websiteinfo", e => "", e => e.Get<string>("url")}};

        private readonly string _tableName;
        public WebsiteInfoServiceAzure(TableContextInterface tableContext
            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService) 
            : base(tableContext, nameAndPartitionProviderService)
        {
            _tableName = nameAndPartitionProviderService.GetTableName<WebsiteInfoEntity>(IdPartition);
        }

        public void RegisterWebsite(string url, WebsiteInfo getWebsiteInfo, bool isDefault = false)
        {
            Action<WebsiteInfoEntity> update
                = registrationEntry =>
                {
                    registrationEntry[FIELD_BEHAIVORTAGS] = getWebsiteInfo.BehaivoirTags.ToString();
                    registrationEntry[FIELD_URL] = url;
                    registrationEntry[FIELD_TAGS] = getWebsiteInfo.Tags.ToString();
                    registrationEntry[FIELD_WEBSITENAME] = getWebsiteInfo.WebsiteName;
                    registrationEntry[FIELD_FACEBOOKAPPID] = getWebsiteInfo.FacebookAppID;
                    registrationEntry[FIELD_FACEBOOKSECRET] = getWebsiteInfo.FacebookAppSecret;
                    registrationEntry[FIELD_PAYPALID] = getWebsiteInfo.PaypalUserId;
                    registrationEntry[FIELD_PAYPALPW] = getWebsiteInfo.PaypalPassword;
                    registrationEntry[FIELD_PAYPALSIG] = getWebsiteInfo.PaypalSignitures;

                    registrationEntry.RowKey = url;
                    registrationEntry.PartitionKey = "";
                    registrationEntry.IsDefault = isDefault;
                };

            var existing = FindById<WebsiteInfoEntity>(url);
            if (existing != null)
                UpdateEntity(url, update);
            else
            {
                var newEntry = new WebsiteInfoEntity();
                update(newEntry);
                Store(newEntry);
            }

            SaveChanges();
        }

        public String GetBehaivourTags(string url)
        {
            var websites = TableContext.PerformQuery<WebsiteInfoEntity>(_tableName);
            var websiteTags = websites
                .Where(_ => _.RowKey == url)
                .Select(_ => _.Get<string>(FIELD_BEHAIVORTAGS)).FirstOrDefault();

            return websiteTags ?? websites.Where(entity => entity.IsDefault).Select(_ => _.Get<string>(FIELD_BEHAIVORTAGS)).FirstOrDefault();
        }

        public String GetTags(string url)
        {
            var websites = TableContext.PerformQuery<WebsiteInfoEntity>(_tableName);
            var tags = websites
                .Where(_ => _.RowKey == url)
                .Select(_ => _.Get<string>(FIELD_TAGS)).FirstOrDefault();

            return tags ?? websites.Where(entity => entity.IsDefault).Select(_ => _.Get<string>(FIELD_TAGS)).FirstOrDefault();
        }

        public string GetWebsiteName(string url)
        {
            var websites = TableContext.PerformQuery<WebsiteInfoEntity>(_tableName);
            var websitename = websites
                .Where(_ => _.RowKey == url)
                .Select(_ => _.Get<string>(FIELD_WEBSITENAME)).FirstOrDefault();

            return websitename ?? websites.Where(entity => entity.IsDefault).Select(_ => _.Get<string>(FIELD_WEBSITENAME)).FirstOrDefault();
        }


        public WebsiteInfo GetWebsiteInfo(string url)
        {
            var websites = TableContext.PerformQuery<WebsiteInfoEntity>(_tableName);

            Expression<Func<WebsiteInfoEntity, WebsiteInfo>> select = _ => new WebsiteInfo()
                {
                    Tags = _.Get<string>(FIELD_TAGS),
                    BehaivoirTags = _.Get<string>(FIELD_BEHAIVORTAGS),
                    FacebookAppID = _.Get<string>(FIELD_FACEBOOKAPPID),
                    FacebookAppSecret = _.Get<string>(FIELD_FACEBOOKSECRET),
                    PaypalPassword = _.Get<string>(FIELD_PAYPALPW),
                    PaypalSignitures = _.Get<string>(FIELD_PAYPALSIG),
                    PaypalUserId = _.Get<string>(FIELD_PAYPALID),
                    WebsiteName = _.Get<string>(FIELD_WEBSITENAME)

                };
            var websiteInfo = websites
                .Where(_ => _.RowKey == url).Select(select).FirstOrDefault();

            return websiteInfo ?? websites.Where(entity => entity.IsDefault).Select(select).FirstOrDefault();
        }


        protected override StorageAggregate GetEntityForUpdate(Type entity, string id)
        {
            var root = FindById(entity, id);
            if (root == null)
                return null;
            var ret = new StorageAggregate(root, NameAndPartitionProviderService);
            ret.LoadAllTableEntriesForUpdate<WebsiteInfoEntity>(TableContext);
            return ret;
        }
    }
}
