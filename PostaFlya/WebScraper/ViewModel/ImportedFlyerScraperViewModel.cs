using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using WebScraper.Library.Model;
using Website.Application.Extension.Content;

namespace WebScraper.ViewModel
{
    public static class ImportedFlyerScraperViewModelExtension
    {
        public static ImportedFlyerScraperViewModel MapFrom(this ImportedFlyerScraperViewModel target,
                                                     ImportedFlyerScraperModel source)
        {
            target.Id = source.Id;

            var stream = source.ImageUrl.ImageStreamFromDataUri();
            if (stream != null)
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = stream;
                bi.EndInit();
                target.Image = bi;
            }
            else if (source.ImageUrl != null)
            {
                target.Image = new BitmapImage(new Uri(source.ImageUrl));
            }



            target.Import = source.IsValid();
            target.Title = source.Title;
            target.Description = source.Description;
            target.VenueInfo = source.VenueInfo;
            target.EventDates = new List<DateTime>(source.EventDates ?? new List<DateTime>());
            target.Tags = source.Tags;
            target.Links = new List<UserLinkViewModel>(source.Links ?? new List<UserLinkViewModel>());
            target.SourceDetailPage = source.SourceDetailPage;

            return target;
        }

        public static ImportedFlyerScraperModel MapFrom(this ImportedFlyerScraperModel target, ImportedFlyerScraperViewModel source)
        {

            target.Title = source.Title;
            target.Description = source.Description;
            target.VenueInfo = source.VenueInfo;
            target.EventDates = new List<DateTime>(source.EventDates);
            target.Tags = source.Tags;
            target.Links = new List<UserLinkViewModel>(source.Links);
            target.Source = source.Source;
            target.SourceDetailPage = source.SourceDetailPage;
            if (source.Image.UriSource != null)
                target.ImageUrl = source.Image.UriSource.ToString();
            if (source.Image.StreamSource != null)
                target.Image = Image.FromStream(source.Image.StreamSource);
            return target;
        }
    }

    public class ImportedFlyerScraperViewModelEqual : IEqualityComparer<ImportedFlyerScraperViewModel>
    {
        public bool Equals(ImportedFlyerScraperViewModel x, ImportedFlyerScraperViewModel y)
        {
            if (x.VenueInfo.PlaceName != y.VenueInfo.PlaceName ||
                x.VenueInfo.SourceId != y.VenueInfo.SourceId ||
                x.Title.ToLower() != y.Title.ToLower() ||
                x.EventDates.Any(xtime => y.EventDates.All(ytime => ytime != xtime)))
                return false;
            return true;
        }

        public int GetHashCode(ImportedFlyerScraperViewModel obj)
        {
            var ret = 0;

            ret = obj.VenueInfo.PlaceName.GetHashCode() ^ obj.VenueInfo.SourceId.GetHashCode();
            ret = ret ^ obj.Title.GetHashCode();
            ret = ret ^ obj.EventDates.Aggregate(0, (i, time) => i ^ time.GetHashCode());

            return ret;
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
        public Uri SourceDetailPage { get; set; }
        public string UserLinksText{get
        {
            if (Links != null && Links.Count > 0)
            {
                return Links.Aggregate("", (s, model) => s + "[" + model.Type + "]");
            }
            return "no links";
        }}

    }
}
