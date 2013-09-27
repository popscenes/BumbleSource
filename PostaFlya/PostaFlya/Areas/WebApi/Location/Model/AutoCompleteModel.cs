using System;
using System.Runtime.Serialization;
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

    [Serializable]
    [DataContract]
    public class AutoCompleteModel : IsModelInterface
    {
        [DataMember]
        public string[] MatchTerms { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Id { get; set; }
    }

}