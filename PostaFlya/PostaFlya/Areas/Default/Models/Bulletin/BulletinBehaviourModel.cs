﻿using System.Collections.Generic;
using System.Linq;
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
            return (flier.Venue != null && flier.Venue.Address != null &&
                    flier.Venue.Address.IsValid)
                       ? flier.Venue.Address.ToViewModel()
                       : flier.Venue.Address.ToViewModel();
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
                EventDates = flier.EventDates,
                CreateDate = flier.CreateDate,
                TagsString = flier.Tags.ToString(),
                FlierImageId = flier.Image.HasValue ? flier.Image.Value.ToString() : null,
                FlierBehaviour = flier.FlierBehaviour.ToString(),
                NumberOfClaims = flier.NumberOfClaims,
                NumberOfComments = flier.NumberOfComments,
                BrowserId = flier.BrowserId,
                ImageList = flier.ImageList.Select(_ => new ImageViewModel() { ImageId = _.ImageID }).ToList(),
                PendingCredits = flier.Features.Sum(_ => _.OutstandingBalance),
                Status = flier.Status.ToString(),
                TinyUrl = flier.TinyUrl,
                UserLinks = flier.UserLinks == null ? new List<UserLinkViewModel>()  : flier.UserLinks.Select(_ => _.ToViewModel()).ToList()

            };
        }
    }

    public class BulletinBehaviourModel
    { 
    }
}