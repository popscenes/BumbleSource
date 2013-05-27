using System.Runtime.Serialization;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;

namespace PostaFlya.Areas.Default.Models
{
    //[KnownType(typeof(TaskJobDetailsViewModel))] //for different behaviour types
    public class DefaultDetailsViewModel
    {
        public BulletinFlierModel Flier { get; set; }
        public FlierAnalyticInfoModel AnalyticInfo { get; set; }
        public VenueInformationModel VenueInformation { get; set; }
        public static DefaultDetailsViewModel DefaultForTemplate()
        {
            var ret = new DefaultDetailsViewModel()
                       {
                           Flier = BulletinFlierModel<BulletinBehaviourModel>.DefaultForTemplate(FlierBehaviour.Default),
                           AnalyticInfo = FlierAnalyticInfoModel.DefaultForTemplate(),
                           VenueInformation = new VenueInformationModel() { Address = new LocationModel() }
                       };
            return ret;
        }
    }
}