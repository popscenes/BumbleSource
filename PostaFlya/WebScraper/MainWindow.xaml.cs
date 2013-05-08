using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
            var listener = new MyTraceListener();
            Trace.Listeners.Add(listener);
            TraceText.DataContext = listener;

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var list = e.Result as List<ImportedFlyerScraperModel>;
            var viewList = list.Select(model => new ImportedFlyerScraperViewModel().MapFrom(model)).ToList();
            ImportListView.ItemsSource = viewList;
            Load.IsEnabled = true;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var startEnd = e.Argument as StartEnd;
            var items = new ConcurrentQueue<ImportedFlyerScraperModel>();
            var all = _kernel.GetAll<SiteScraperInterface>();
            foreach (var siteScraper in all)
            {
                Trace.TraceInformation("Running " + siteScraper.SiteName);
                var next = siteScraper.GetFlyersFrom(startEnd.Start, startEnd.End);
                foreach (var importedFlyerScraperModel in next.Where(model => model.EventDates.All(time => time <= startEnd.End)))
                {
                    items.Enqueue(importedFlyerScraperModel);
                }

            }

            e.Result = items.ToList();
        }

        private readonly BackgroundWorker worker = new BackgroundWorker();


        private class StartEnd
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }
        private void Load_Click(object sender, RoutedEventArgs ev)
        {

            if (!StartDate.SelectedDate.HasValue || !EndDate.SelectedDate.HasValue)
            {
                Trace.TraceWarning("No Date Set");
                return;
            }
            Load.IsEnabled = false;

            Trace.TraceInformation("Starting...");

            worker.RunWorkerAsync(new StartEnd()
                {
                    Start = StartDate.SelectedDate.Value,
                    End = EndDate.SelectedDate.Value
                });




            

           
//                = new List<ImportedFlyerScraperViewModel>()
//                {
//                    new ImportedFlyerScraperViewModel()
//                        {
//                            Import = true,
//                            Title = "Test",
//                            Description = "Test Desc",
//                            Image =  new BitmapImage(new Uri("http://www.google.com.au/images/srpr/logo4w.png")),
//                            EventDates = new List<DateTime>()
//                                {
//                                    DateTime.Now.AddDays(-1),
//                                    DateTime.Now.AddDays(1)
//                                },
//                                Location = new LocationScraperModel()
//                                    {
//                                       Longitude = 143,
//                                       Latitude = 93
//                                    },
//                                    VenueInfo = new VenueInfoScraperModel()
//                                        {
//                                            PlaceName = "Test Place",
//                                            SourceId = "12354"
//                                        }
//                        },
//                                            new ImportedFlyerScraperViewModel()
//                        {
//                            Import = true,
//                            Title = "Test",
//                            Description = "Test Desc",
//                            Image =  new BitmapImage(new Uri("http://www.google.com.au/images/srpr/logo4w.png")),
//                            EventDates = new List<DateTime>()
//                                {
//                                    DateTime.Now.AddDays(-1),
//                                    DateTime.Now.AddDays(1)
//                                },
//                                Location = new LocationScraperModel()
//                                    {
//                                       Longitude = 143,
//                                       Latitude = 93
//                                    },
//                                    VenueInfo = new VenueInfoScraperModel()
//                                        {
//                                            PlaceName = "Test Place",
//                                            SourceId = "12354"
//                                        }
//                        }
//                };
        }
    }

    public class MyTraceListener : TraceListener, INotifyPropertyChanged
    {
        private readonly StringBuilder builder;

        public MyTraceListener()
        {
            this.builder = new StringBuilder();
        }

        public string Trace
        {
            get { return this.builder.ToString(); }
        }

        public override void Write(string message)
        {
            this.builder.Append(message);
            this.OnPropertyChanged(new PropertyChangedEventArgs("Trace"));
        }

        public override void WriteLine(string message)
        {
            this.builder.AppendLine(message);
            this.OnPropertyChanged(new PropertyChangedEventArgs("Trace"));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
