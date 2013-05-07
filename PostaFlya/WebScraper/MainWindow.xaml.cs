using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ninject;
using WebScraper.Library.Infrastructure;
using WebScraper.Library.Model;
using WebScraper.Library.Sites;
using WebScraper.ViewModel;
using Image = System.Drawing.Image;

namespace WebScraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public StandardKernel _kernel;
        public MainWindow()
        {
            InitializeComponent();
            _kernel = new StandardKernel();
            _kernel.Load<RegisterSites>();
        }

        private void Load_Click(object sender, RoutedEventArgs ev)
        {

            var all = _kernel.GetAll<SiteScraperInterface>();
            foreach (var siteScraper in all)
            {
                var state = State.Site[siteScraper.SiteName] ?? new SiteState(){LastGigDateUpdated = DateTime.Today.AddDays(-1)};
                siteScraper.GetFlyersFrom(state.LastGigDateUpdated);
            }

            ImportListView.ItemsSource = new List<ImportedFlyerScraperViewModel>()
                {
                    new ImportedFlyerScraperViewModel()
                        {
                            Import = true,
                            Title = "Test",
                            Description = "Test Desc",
                            Image =  new BitmapImage(new Uri("http://www.google.com.au/images/srpr/logo4w.png")),
                            EventDates = new List<DateTime>()
                                {
                                    DateTime.Now.AddDays(-1),
                                    DateTime.Now.AddDays(1)
                                },
                                Location = new LocationScraperModel()
                                    {
                                       Longitude = 143,
                                       Latitude = 93
                                    },
                                    VenueInfo = new VenueInfoScraperModel()
                                        {
                                            PlaceName = "Test Place",
                                            SourceId = "12354"
                                        }
                        },
                                            new ImportedFlyerScraperViewModel()
                        {
                            Import = true,
                            Title = "Test",
                            Description = "Test Desc",
                            Image =  new BitmapImage(new Uri("http://www.google.com.au/images/srpr/logo4w.png")),
                            EventDates = new List<DateTime>()
                                {
                                    DateTime.Now.AddDays(-1),
                                    DateTime.Now.AddDays(1)
                                },
                                Location = new LocationScraperModel()
                                    {
                                       Longitude = 143,
                                       Latitude = 93
                                    },
                                    VenueInfo = new VenueInfoScraperModel()
                                        {
                                            PlaceName = "Test Place",
                                            SourceId = "12354"
                                        }
                        }
                };
        }
    }
}
