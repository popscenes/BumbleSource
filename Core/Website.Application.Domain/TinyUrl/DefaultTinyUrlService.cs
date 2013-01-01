using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Website.Application.Binding;
using Website.Application.Queue;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Application.Domain.TinyUrl
{
    public class DefaultTinyUrlService : TinyUrlServiceInterface
    {
        private readonly QueueInterface _tinyUrlQueue;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;

        public DefaultTinyUrlService([TinyUrlQueue]QueueInterface tinyUrlQueue,
            UnitOfWorkFactoryInterface unitOfWorkFactory, GenericRepositoryInterface repository,
            GenericQueryServiceInterface queryService)
        {
            _tinyUrlQueue = tinyUrlQueue;
            _unitOfWorkFactory = unitOfWorkFactory;
            _repository = repository;
            _queryService = queryService;
        }

        private string NewUrl()
        {
            var msg = _tinyUrlQueue.GetMessage();
            if (msg == null)
            {
                Trace.TraceError("DefaultTinyUrlService: TinyUrl Queue does not contain any urls");
                return null;
            }

            var ret = System.Text.Encoding.ASCII.GetString(msg.Bytes);
            _tinyUrlQueue.DeleteMessage(msg);
            return ret;
        }

        public string UrlFor<UrlEntityType>(UrlEntityType entity) where UrlEntityType : TinyUrlInterface, EntityInterface
        {
            var ret = _queryService.FindAggregateEntities<TinyUrlRecord>(entity.Id);
            var record = ret.SingleOrDefault();
            if (record != null)
                return record.TinyUrl;

            var url = NewUrl();
            record = new TinyUrlRecord()
                {
                    AggregateId = entity.Id,
                    AggregateTypeTag = entity.PrimaryInterface.AssemblyQualifiedName,
                    Id = HttpUtility.UrlEncode(url),
                    TinyUrl = url,
                    FriendlyId = entity.FriendlyId
                };

            var uow = _unitOfWorkFactory.GetUnitOfWork(new[] {_repository});
            using (uow)
            {
                _repository.Store(record);
            }
            if (!uow.Successful)
            {
                Trace.TraceError("Failed to save TinyUrl");
                return null;
            }

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
