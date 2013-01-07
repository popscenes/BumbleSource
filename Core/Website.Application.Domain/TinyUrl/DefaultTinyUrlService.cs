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
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Application.Domain.TinyUrl
{
    public class DefaultTinyUrlService : TinyUrlServiceInterface
    {
        public const string UnassignedToAggregate = "unassigned";
        public const int TinyUrlsToBuffer = 2000;

        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;

        public DefaultTinyUrlService(
            UnitOfWorkFactoryInterface unitOfWorkFactory, GenericRepositoryInterface repository,
            GenericQueryServiceInterface queryService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repository = repository;
            _queryService = queryService;
        }

        private Random _picker;
        private TinyUrlRecord NewUrl()
        {
            var unassigned = _queryService.FindAggregateEntityIds<TinyUrlRecord>(UnassignedToAggregate).ToList();
            if (!unassigned.Any())
            {
                Trace.TraceError("DefaultTinyUrlService: TinyUrl table does not contain any free urls");
                return null;
            }
            
            var idxToTry = _picker.Next(0, unassigned.Count() - 1);

            return _queryService.FindById<TinyUrlRecord>(unassigned[idxToTry]);
        }

        public string UrlFor<UrlEntityType>(UrlEntityType entity) where UrlEntityType : TinyUrlInterface, EntityInterface
        {
            var ret = _queryService.FindAggregateEntities<TinyUrlRecord>(entity.Id);
            var record = ret.SingleOrDefault();
            if (record != null)
                return record.TinyUrl;

            _picker = new Random(entity.Id.GetHashCode());
            var gotone = true;
            do
            {
                gotone = true;
                var uow = _unitOfWorkFactory.GetUnitOfWork(new[] {_repository});
                using (uow)
                {
                    record = NewUrl();
                    _repository.UpdateEntity<TinyUrlRecord>(record.Id,
                        urlRecord =>
                            {
                                if (!urlRecord.AggregateId.Equals(UnassignedToAggregate))
                                {
                                    gotone = false;
                                    return;
                                }
                                urlRecord.AggregateId = entity.Id;
                                urlRecord.AggregateTypeTag =
                                    entity.PrimaryInterface.AssemblyQualifiedName;
                                urlRecord.FriendlyId = entity.FriendlyId;
                            }
                        );
                }

                if (uow.Successful) continue;
                Trace.TraceError("Failed to save TinyUrl");
                gotone = false;
            } while (!gotone);


            return record.TinyUrl;

        }

        public EntityInterface EntityInfoFor(string url)
        {
            var record = _queryService.FindById<TinyUrlRecord>(HttpUtility.UrlEncode(url));
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
