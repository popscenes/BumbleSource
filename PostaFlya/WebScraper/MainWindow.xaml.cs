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
using WebScraper.Library.Model;
using WebScraper.ViewModel;
using Image = System.Drawing.Image;

namespace WebScraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load_Click(object sender, RoutedEventArgs ev)
        {
//            Image img = null;
//            var cli = new HttpClient();
//
//            var clientResponse = cli.GetByteArrayAsync(new Uri("http://www.google.com.au/images/srpr/logo4w.png"));
//            //clientResponse.RunSynchronously();
//            using (var ms = new MemoryStream(clientResponse.Result, false))
//            {
//                try
//                {
//                    img = Image.FromStream(ms);
//                }
//                catch (Exception e)
//                {
//                    Trace.TraceInformation("ImageProcessCommandHandler Error: {0}, Stack {1}", e.Message, e.StackTrace);
//                }
//
//            }

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
