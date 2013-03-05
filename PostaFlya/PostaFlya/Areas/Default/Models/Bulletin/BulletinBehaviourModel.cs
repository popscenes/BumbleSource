using System.Linq;
using System.Runtime.Serialization;
using PostaFlya.Areas.TaskJob.Models.Bulletin;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Content;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;

namespace PostaFlya.Areas.Default.Models.Bulletin
{
    public static class FlierBehaviourInterfaceExtensions
    {
        public static LocationModel LocationModelForFlier(this FlierInterface flier)
        {
            return (flier.ContactDetails != null && flier.ContactDetails.Address != null &&
                    flier.ContactDetails.Address.IsValid)
                       ? flier.ContactDetails.Address.ToViewModel()
                       : flier.Location.ToViewModel();
        }
        public static BulletinFlierModel<BulletinBehaviourType> ToViewModel<BulletinBehaviourType>(this FlierInterface flier, bool detailMode) where BulletinBehaviourType : new()
        {
            return new BulletinFlierModel<BulletinBehaviourType>()
            {
                Id = flier.Id,
                FriendlyId = flier.FriendlyId,
                Title = flier.Title,
                Description = (!detailMode && !string.IsNullOrWhiteSpace(flier.Description)) ? (flier.Description.Length > 200 ? flier.Description.Substring(0, 200) : flier.Description) : flier.Description,
                Location = flier.LocationModelForFlier(),
                EffectiveDate = flier.EffectiveDate,
                CreateDate = flier.CreateDate,
                TagsString = flier.Tags.ToString(),
                FlierImageId = flier.Image.HasValue ? flier.Image.Value.ToString() : null,
                FlierBehaviour = flier.FlierBehaviour.ToString(),
                NumberOfClaims = flier.NumberOfClaims,
                NumberOfComments = flier.NumberOfComments,
                BrowserId = flier.BrowserId,
                ImageList = flier.ImageList.Select(_ => new ImageViewModel() { ImageId = _.ImageID }).ToList(),
                PendingCredits = flier.Features.Sum(_ => _.OutstandingBalance)

            };
        }
    }

    public class BulletinBehaviourModel
    { 
    }
}