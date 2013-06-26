using System;
using System.Collections.Generic;
using System.Drawing;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Domain.Location;

namespace WebScraper.Library.Model
{
    public static class ImportedFlyerScraperModelExtensions
    {
        public static FlierCreateModel MapFrom(this FlierCreateModel target, ImportedFlyerScraperModel source)
        {
            target.FlierImageId = source.ImageUrl;
            target.TagsString = source.Tags;
            target.Title = source.Title;
            target.Description = source.Description;
            target.EventDates = source.EventDates;
            target.Anonymous = true;
            //target.VenueInformation = source.VenueInfo;
            target.BoardList = new List<string>(){source.BoardId};
            target.UserLinks = source.Links;
            return target;
        }
    }
    public class ImportedFlyerScraperModel
    {
        public Guid Id { get; set; }   
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<DateTime> EventDates { get; set; }
        public List<UserLinkViewModel> Links { get; set; }
        public string Tags { get; set; }
        public string Source { get; set; }
        public Uri SourceDetailPage { get; set; }
        public string BoardId { get; set; }

        public VenueInformationModel VenueInfo { get; set; }

        public Image Image { get; set; }

        public bool IsValid()
        {

            var ret = true;

            try
            {
                ret = ret && !string.IsNullOrWhiteSpace(Title);
                ret = ret && !string.IsNullOrWhiteSpace(Description);
                ret = ret && !string.IsNullOrWhiteSpace(ImageUrl);
                ret = ret && !string.IsNullOrWhiteSpace(Tags);
                ret = ret && !string.IsNullOrWhiteSpace(BoardId); 
                ret = ret && EventDates.Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
            

            return ret;
        }

    }
}
