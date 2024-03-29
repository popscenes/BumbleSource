//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Web;
//using Website.Application.Queue;
//using Website.Application.Schedule;
//using Website.Infrastructure.Command;
//using Website.Infrastructure.Configuration;
//using Website.Infrastructure.Query;
//
//namespace Website.Application.Domain.TinyUrl
//{
//    public class TinyUrlGenerationJobAction : JobActionInterface
//    {
//        private const string UrlBase = "urlbase";
//        private const string StartPath = "startpath";
//
//        public static Dictionary<string, string> GetDefaults(ConfigurationServiceInterface configurationService)
//        {
//            var url = configurationService != null ? configurationService.GetSetting("TinyUrlBase") : null;
//            if (string.IsNullOrEmpty(url))
//                url = "http://pfly.in/";
//            return new Dictionary<string, string>() { { UrlBase, url }, { StartPath, "" } };
//        }
//        private readonly GenericQueryServiceInterface _queryService;
//        private readonly GenericRepositoryInterface _repository;
//        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
//        private readonly ConfigurationServiceInterface _configurationService;
//
//        public TinyUrlGenerationJobAction(GenericQueryServiceInterface queryService = null,
//                                GenericRepositoryInterface repository = null, UnitOfWorkFactoryInterface unitOfWorkFactory = null
//                                , ConfigurationServiceInterface configurationService = null)
//        {
//            _queryService = queryService;
//            _repository = repository;
//            _unitOfWorkFactory = unitOfWorkFactory;
//            _configurationService = configurationService;
//        }
//
//        public void Run(JobBase job)
//        {
//            var unassigned = _queryService.FindAggregateEntityIds<TinyUrlRecord>(TinyUrlRecord.UnassignedToAggregateId).Count();
//            if (unassigned >= DefaultTinyUrlService.TinyUrlsToBuffer)
//                return;
//
//            if (job.JobStorage == null || !job.JobStorage.ContainsKey(UrlBase))
//                job.JobStorage = GetDefaults(_configurationService);
//
//            var last = job.JobStorage[StartPath];
//            var uow = _unitOfWorkFactory.GetUnitOfWork(new[] {_repository});
//            using (uow)
//            {              
//                for (var i = 0; i < 500; i++)
//                {
//                    last = AddNewUrl(job.JobStorage[UrlBase], last);
//                }
//            }
//
//            if (uow.Successful)
//            {
//                job.JobStorage[StartPath] = last;
//            }
//            else
//            {
//                Trace.TraceWarning("Tiny Url Generation Failed");
//            }
//        }
//
//        private string AddNewUrl(string baseUrl, string start)
//        {
//            var next = TinyUrlUtil.Increment(start);
//
//            _repository.Store(new TinyUrlRecord()
//                {
//                    AggregateId = TinyUrlRecord.UnassignedToAggregateId,
//                    AggregateTypeTag = "",
//                    FriendlyId = "",
//                    Id = TinyUrlRecord.GenerateIdFromUrl(baseUrl + next),
//                    TinyUrl = baseUrl + next
//                });
//
//            return next;
//        }
//
//    }
//}