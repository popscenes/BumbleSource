using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.DataRepository.Binding;
using Website.Azure.Common.TableStorage;
using Website.Domain.Query;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.DataRepository.DomainQuery
{
    public class AutoCompleteByTermsQueryHandler : QueryHandlerInterface<AutoCompleteByTermsQuery, List<SearchEntityRecord>>
    {
        private readonly TableIndexServiceInterface _indexService;

        public AutoCompleteByTermsQueryHandler(TableIndexServiceInterface indexService)
        {
            _indexService = indexService;
        }

        public List<SearchEntityRecord> Query(AutoCompleteByTermsQuery argument)
        {
            var prefix = argument.Terms.TokenizeMeaningfulWordsAndSort();
            var entries = _indexService.FindEntitiesByIndexPrefix<EntityInterface, JsonTableEntry>(DomainIndexSelectors.TextSearchIndex, prefix);
            return entries.Select(e => e.GetEntity() as SearchEntityRecord).ToList();
        }
    }
}
