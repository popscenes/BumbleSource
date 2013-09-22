using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Website.Application.Domain.TinyUrl.Query;
using Website.Application.Queue;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Command;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Application.Domain.TinyUrl
{
    public class DefaultTinyUrlService : TinyUrlServiceInterface
    {
        public const int TinyUrlsToBuffer = 2000;

        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly ConfigurationServiceInterface _configurationService;
        private readonly QueryChannelInterface _queryChannel;

        public DefaultTinyUrlService(GenericRepositoryInterface repository,
            GenericQueryServiceInterface queryService, ConfigurationServiceInterface configurationService
            , QueryChannelInterface queryChannel)
        {
            _repository = repository;
            _queryService = queryService;
            _configurationService = configurationService;
            _queryChannel = queryChannel;
        }

        private Random _picker;
        private TinyUrlRecord BaseUrl()
        {
            var baseUrls = _queryService.FindAggregateEntityIds<TinyUrlRecord>(TinyUrlRecord.UnassignedToAggregateId).ToList();
            if (!baseUrls.Any())
            {
                return Init();
            }
            
            var idxToTry = _picker.Next(0, baseUrls.Count() - 1);

            return _queryService.FindByAggregate<TinyUrlRecord>(baseUrls[idxToTry], TinyUrlRecord.UnassignedToAggregateId);
        }

        private TinyUrlRecord Init()
        {
            var url = _configurationService.GetSetting("TinyUrlBase");
            if (string.IsNullOrEmpty(url))
                url = "http://pfly.in/";
            var rec = new TinyUrlRecord()
                {
                    AggregateId = TinyUrlRecord.UnassignedToAggregateId,
                    AggregateTypeTag = "",
                    FriendlyId = "",
                    Id = TinyUrlRecord.GenerateIdFromUrl(url),
                    TinyUrl = url
                };


            _repository.Store(rec);    
            
            return rec;
        }

        public string UrlFor<UrlEntityType>(UrlEntityType entity) where UrlEntityType : class, EntityWithTinyUrlInterface, new()
        {
            var ent = _queryService.FindById<UrlEntityType>(entity.Id);
            if (ent != null && !string.IsNullOrEmpty(ent.TinyUrl))
                return ent.TinyUrl;

            _picker = new Random(entity.Id.GetHashCode());

            var newUrl = "";

            var record = BaseUrl();

            //this will retry until it has successfully incremented the url
            _repository.UpdateAggregateEntity<TinyUrlRecord>(record.Id, record.AggregateId,
                urlRecord =>
                    {
                        TinyUrlUtil.Increment(urlRecord);
                        newUrl = urlRecord.TinyUrl;
                    }
                );
            
            return newUrl;

        }

        public EntityInterface EntityInfoFor(string url)
        {
            return _queryChannel.Query(new FindByTinyUrlQuery() { Url = url }, (EntityWithTinyUrlInterface)null);
        }

    }


}
