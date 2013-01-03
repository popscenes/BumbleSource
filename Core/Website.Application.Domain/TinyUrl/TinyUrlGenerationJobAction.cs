using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Application.Binding;
using Website.Application.Queue;
using Website.Application.Schedule;
using Website.Infrastructure.Command;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace Website.Application.Domain.TinyUrl
{
    public class TinyUrlGenerationJobAction : JobActionInterface
    {
        private const string UrlBase = "urlbase";
        private const string StartPath = "startpath";

        public static Dictionary<string, string> GetDefaults(ConfigurationServiceInterface configurationService)
        {
            var url = configurationService != null ? configurationService.GetSetting("TinyUrlBase") : null;
            if (string.IsNullOrEmpty(url))
                url = "http://pfly.in/";
            return new Dictionary<string, string>() { { UrlBase, url }, { StartPath, "" } };
        }
        private readonly QueueInterface _urlQueue;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly ConfigurationServiceInterface _configurationService;

        public TinyUrlGenerationJobAction([TinyUrlQueue]QueueInterface urlQueue, GenericQueryServiceInterface queryService = null,
                                GenericRepositoryInterface repository = null, UnitOfWorkFactoryInterface unitOfWorkFactory = null
                                , ConfigurationServiceInterface configurationService = null)
        {
            _urlQueue = urlQueue;
            _queryService = queryService;
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _configurationService = configurationService;
        }

        public void Run(JobBase job)
        {
//            var unassigned = _queryService.FindAggregateEntityIds<TinyUrlRecord>("");
//            if (unassigned.Count() >= 5000)
//                return;

            if (!_urlQueue.ApproximateMessageCount.HasValue || _urlQueue.ApproximateMessageCount.Value > 500)
                return;

            if (job.JobStorage == null || !job.JobStorage.ContainsKey(UrlBase))
                job.JobStorage = GetDefaults(_configurationService);

            for (var i = 0; i < 500; i++)
            {
                AddNewUrlToQueue(job);
            }
        }

        private void AddNewUrlToQueue(JobBase job)
        {

            var start = job.JobStorage[StartPath];
            var next = Increment(start);

            var uow = _unitOfWorkFactory.GetUnitOfWork(new[] {_repository});
            using (uow)
            {
                
            }

            _urlQueue.AddMessage(
                new QueueMessage(System.Text.Encoding.ASCII.GetBytes(job.JobStorage[UrlBase] + next))
                );

            job.JobStorage[StartPath] = next;

        }

        private static readonly char[] UrlChars;

        static TinyUrlGenerationJobAction()
        {
            var ret = new List<char>();
            for (var i = '0'; i <= '9'; i++)
            {
                ret.Add(i);
            }

            for (var i = 'a'; i <= 'z'; i++)
            {
                ret.Add(i);
            }

            UrlChars = ret.ToArray();
        }

        private static int[] InitCounter(string source)
        {
            source = '+' + source; //add bogus char to initialize first element to -1 for rollover space
            return source.Select(chr => Array.IndexOf<char>(UrlChars, chr)).ToArray();
        }

        private static string Increment(string start)
        {
            var current = InitCounter(start);
            for (var i = current.Length - 1; i >= 0; i--)
            {
                current[i] = (current[i] + 1)%UrlChars.Length;
                if (current[i] != 0)
                    break;
            }

            return current.Where(i => i >= 0)
                   .Select(i => UrlChars[i])
                   .Aggregate(new StringBuilder(), (builder, c) => builder.Append(c))
                   .ToString();
        }
    }
}