using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using WebScraper.Library.Model;

namespace WebScraper.ViewModel
{
    public static class ImportedFlyerScraperViewModelExtension
    {
        public static ImportedFlyerScraperViewModel MapFrom(this ImportedFlyerScraperViewModel target,
                                                     ImportedFlyerScraperModel source)
        {
            target.Id = source.Id;
            target.Image = new BitmapImage(new Uri(source.ImageUrl));
            target.Import = source.IsValid();
            target.Title = source.Title;
            target.Description = source.Description;
            target.VenueInfo = source.VenueInfo;
            target.EventDates = new List<DateTime>(source.EventDates ?? new List<DateTime>());
            target.Tags = source.Tags;
            target.Links = new List<UserLinkViewModel>(source.Links ?? new List<UserLinkViewModel>());
            return target;
        }

        public static ImportedFlyerScraperModel MapTo(this ImportedFlyerScraperViewModel source,
                                             ImportedFlyerScraperModel target)
        {

            target.Title = source.Title;
            target.Description = source.Description;
            target.VenueInfo = source.VenueInfo;
            target.EventDates = new List<DateTime>(source.EventDates);
            target.Tags = source.Tags;
            target.Links = new List<UserLinkViewModel>(source.Links);
            target.Source = source.Source;
            return target;
        }
    }
    public class ImportedFlyerScraperViewModel
    {
        public Guid Id { get; set; }   
        public bool Import { get; set; }
        public BitmapImage Image { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<DateTime> EventDates { get; set; }
        public List<UserLinkViewModel> Links { get; set; }        
        public VenueInformationModel VenueInfo { get; set; }
        public string Tags { get; set; }
        public string Source { get; set; }

    }
}
