using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;

namespace PostaFlya.Areas.Default.Models
{
    //[KnownType(typeof(TaskJobDetailsViewModel))] //for different behaviour types
    public class DefaultDetailsViewModel
    {
        public BulletinFlierDetailModel Flier { get; set; }

        public static DefaultDetailsViewModel DefaultForTemplate()
        {
            var ret = new DefaultDetailsViewModel()
                       {
                           Flier = new BulletinFlierDetailModel(){Venue = new VenueInformationModel()}
                       };
            return ret;
        }
    }
}