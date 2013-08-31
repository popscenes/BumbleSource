using Website.Common.Model;
using Website.Domain.Query;

namespace PostaFlya.Areas.WebApi.Location.Model
{
    public class ToAutoCompleteModel : ViewModelMapperInterface<AutoCompleteModel, SearchEntityRecord>
    {
        public AutoCompleteModel ToViewModel(AutoCompleteModel target, SearchEntityRecord source)
        {
            if(target == null)
                target = new AutoCompleteModel();

            target.Url = source.FriendlyId;
            target.Type = source.TypeOfEntity;
            target.Description = source.DisplayString;
            target.MatchTerms = source.SearchTerms;
            target.Id = source.Id;
            return target;
        }
    }

    public class AutoCompleteModel : IsModelInterface
    {
        public string[] MatchTerms { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
    }

}