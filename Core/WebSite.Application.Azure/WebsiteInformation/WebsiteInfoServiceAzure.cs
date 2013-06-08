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
    public class WebsiteInfoEntity : SimpleExtendableEntity, AggregateRootInterface
    {
        internal const string FIELD_BEHAIVORTAGS = "behaivourtags";
        internal const string FIELD_URL = "url";
        internal const string FIELD_TAGS = "tags";
        internal const string FIELD_WEBSITENAME = "websitename";
        internal const string FIELD_FACEBOOKAPPID = "facebookappid";
        internal const string FIELD_FACEBOOKSECRET = "facebooksecret";

        internal const string FIELD_PAYPALID = "paypalId";
        internal const string FIELD_PAYPALPW = "paypalPw";
        internal const string FIELD_PAYPALSIG = "paypalSig";
        internal const string FIELD_ISDEFAULT = "IsDefault";

        public bool IsDefault()
        {
            return (bool) (this[FIELD_ISDEFAULT] ?? false);
        }
    }

    public class WebsiteInfoServiceAzure : RepositoryBase<WebsiteInfoEntity>, WebsiteInfoServiceInterface
    {
//        public static TableNameAndPartitionProvider<SimpleExtendableEntity> TableNameBinding
//            = new TableNameAndPartitionProvider<SimpleExtendableEntity>()
//                  {{typeof (SimpleExtendableEntity), 0, "websiteinfo", e => "", e => e.Get<string>("url")}};

        private readonly string _tableName;
        public WebsiteInfoServiceAzure(TableContextInterface tableContext
            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService) 
            : base(tableContext, nameAndPartitionProviderService)
        {
            _tableName = nameAndPartitionProviderService.GetTableName<WebsiteInfoEntity>();
        }

        public void RegisterWebsite(string url, WebsiteInfo getWebsiteInfo, bool isDefault = false)
        {
            Action<WebsiteInfoEntity> update
                = registrationEntry =>
                {
                    registrationEntry[WebsiteInfoEntity.FIELD_BEHAIVORTAGS] = getWebsiteInfo.BehaivoirTags.ToString();
                    registrationEntry[WebsiteInfoEntity.FIELD_URL] = url;
                    registrationEntry[WebsiteInfoEntity.FIELD_TAGS] = getWebsiteInfo.Tags.ToString();
                    registrationEntry[WebsiteInfoEntity.FIELD_WEBSITENAME] = getWebsiteInfo.WebsiteName;
                    registrationEntry[WebsiteInfoEntity.FIELD_FACEBOOKAPPID] = getWebsiteInfo.FacebookAppID;
                    registrationEntry[WebsiteInfoEntity.FIELD_FACEBOOKSECRET] = getWebsiteInfo.FacebookAppSecret;
                    registrationEntry[WebsiteInfoEntity.FIELD_PAYPALID] = getWebsiteInfo.PaypalUserId;
                    registrationEntry[WebsiteInfoEntity.FIELD_PAYPALPW] = getWebsiteInfo.PaypalPassword;
                    registrationEntry[WebsiteInfoEntity.FIELD_PAYPALSIG] = getWebsiteInfo.PaypalSignitures;
                    registrationEntry[WebsiteInfoEntity.FIELD_ISDEFAULT] = isDefault;

                    registrationEntry.RowKey = url;
                    registrationEntry.PartitionKey = "";
                    
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
                .Select(_ => _.Get<string>(WebsiteInfoEntity.FIELD_BEHAIVORTAGS)).FirstOrDefault();

            return websiteTags ?? websites.Where(entity => entity.IsDefault()).Select(_ => _.Get<string>(WebsiteInfoEntity.FIELD_BEHAIVORTAGS)).FirstOrDefault();
        }

        public String GetTags(string url)
        {
            var websites = TableContext.PerformQuery<WebsiteInfoEntity>(_tableName);
            var tags = websites
                .Where(_ => _.RowKey == url)
                .Select(_ => _.Get<string>(WebsiteInfoEntity.FIELD_TAGS)).FirstOrDefault();

            return tags ?? websites.Where(entity => entity.IsDefault()).Select(_ => _.Get<string>(WebsiteInfoEntity.FIELD_TAGS)).FirstOrDefault();
        }

        public string GetWebsiteName(string url)
        {
            var websites = TableContext.PerformQuery<WebsiteInfoEntity>(_tableName);
            var websitename = websites
                .Where(_ => _.RowKey == url)
                .Select(_ => _.Get<string>(WebsiteInfoEntity.FIELD_WEBSITENAME)).FirstOrDefault();

            return websitename ?? websites.Where(entity => entity.IsDefault()).Select(_ => _.Get<string>(WebsiteInfoEntity.FIELD_WEBSITENAME)).FirstOrDefault();
        }


        public WebsiteInfo GetWebsiteInfo(string url)
        {
            var websites = TableContext.PerformQuery<WebsiteInfoEntity>(_tableName);

            Expression<Func<WebsiteInfoEntity, WebsiteInfo>> select = _ => new WebsiteInfo()
                {
                    Tags = _.Get<string>(WebsiteInfoEntity.FIELD_TAGS),
                    BehaivoirTags = _.Get<string>(WebsiteInfoEntity.FIELD_BEHAIVORTAGS),
                    FacebookAppID = _.Get<string>(WebsiteInfoEntity.FIELD_FACEBOOKAPPID),
                    FacebookAppSecret = _.Get<string>(WebsiteInfoEntity.FIELD_FACEBOOKSECRET),
                    PaypalPassword = _.Get<string>(WebsiteInfoEntity.FIELD_PAYPALPW),
                    PaypalSignitures = _.Get<string>(WebsiteInfoEntity.FIELD_PAYPALSIG),
                    PaypalUserId = _.Get<string>(WebsiteInfoEntity.FIELD_PAYPALID),
                    WebsiteName = _.Get<string>(WebsiteInfoEntity.FIELD_WEBSITENAME)

                };
            var websiteInfo = websites
                .Where(_ => _.RowKey == url).Select(select).FirstOrDefault();

            return websiteInfo ?? websites.Where(entity => entity.IsDefault()).Select(select).FirstOrDefault();
        }

    }
}
