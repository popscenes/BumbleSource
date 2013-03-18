using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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

        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly ConfigurationServiceInterface _configurationService;

        public DefaultTinyUrlService(
            UnitOfWorkFactoryInterface unitOfWorkFactory, GenericRepositoryInterface repository,
            GenericQueryServiceInterface queryService, ConfigurationServiceInterface configurationService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repository = repository;
            _queryService = queryService;
            _configurationService = configurationService;
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

            return _queryService.FindById<TinyUrlRecord>(baseUrls[idxToTry]);
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

            var uow = _unitOfWorkFactory.GetUnitOfWork(new object[] {_repository});
            using (uow)
            {
                _repository.Store(rec);    
            }
            return rec;
        }

        public string UrlFor<UrlEntityType>(UrlEntityType entity) where UrlEntityType : TinyUrlInterface, EntityInterface
        {
            var ret = _queryService.FindAggregateEntities<TinyUrlRecord>(entity.Id);
            var record = ret.SingleOrDefault();
            if (record != null)
                return record.TinyUrl;

            _picker = new Random(entity.Id.GetHashCode());

            var newUrl = "";
            var uow = _unitOfWorkFactory.GetUnitOfWork(new object[] {_repository});
            using (uow)
            {
                record = BaseUrl();
                
                //this will retry until it has successfully incremented the url
                _repository.UpdateEntity<TinyUrlRecord>(record.Id,
                    urlRecord =>
                        {
                            TinyUrlUtil.Increment(urlRecord);
                            newUrl = urlRecord.TinyUrl;
                        }
                    );
            }

            if (!uow.Successful)
            {
                Trace.TraceError("Failed to save TinyUrl");
                return null;
            }

            var newRec = new TinyUrlRecord()
                {
                    AggregateId = entity.Id,
                    AggregateTypeTag = entity.PrimaryInterface.AssemblyQualifiedName,
                    FriendlyId = entity.FriendlyId,
                    Id = TinyUrlRecord.GenerateIdFromUrl(newUrl),
                    TinyUrl = newUrl
                };
            uow = _unitOfWorkFactory.GetUnitOfWork(new object[] {_repository});
            using (uow)
            {
                _repository.Store(newRec);
            }

            return newRec.TinyUrl;

        }

        public EntityInterface EntityInfoFor(string url)
        {
            var record = _queryService.FindById<TinyUrlRecord>(TinyUrlRecord.GenerateIdFromUrl(url));
            if (record == null)
                return null;
            return new TinyUrlServiceEntityInformation()
                {
                  Id = record.AggregateId,
                  PrimaryInterface = Type.GetType(record.AggregateTypeTag),
                  FriendlyId = record.FriendlyId
                };
        }

        public class TinyUrlServiceEntityInformation : EntityInterface
        {
            public string Id { get; set; }
            public string FriendlyId { get; set; }
            public int Version { get; set; }
            public Type PrimaryInterface { get; set; }
        }
    }
}
