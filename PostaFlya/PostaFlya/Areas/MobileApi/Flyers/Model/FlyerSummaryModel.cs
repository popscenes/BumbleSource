using PostaFlya.Domain.Flier;
using Website.Common.Model;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{
    public class ToFlyerSummaryModel : ViewModelMapperInterface<FlyerSummaryModel, Flier>
    {
        public FlyerSummaryModel ToViewModel(FlyerSummaryModel target, Flier source)
        {
            if(target == null)
                target = new FlyerSummaryModel();
            
            return target;

        }
    }
    public class FlyerSummaryModel
    {
    }
}