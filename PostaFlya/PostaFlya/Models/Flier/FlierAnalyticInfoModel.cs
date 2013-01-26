using PostaFlya.Domain.Flier.Analytic;

namespace PostaFlya.Models.Flier
{
    public static class FlierAnalyticInfoExtensions
    {
        public static FlierAnalyticInfoModel ToModel(this FlierAnalyticInfo info)
        {
            return new FlierAnalyticInfoModel();
        }
    }
    public class FlierAnalyticInfoModel
    {

    }
}