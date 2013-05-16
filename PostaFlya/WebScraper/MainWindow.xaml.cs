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
using System.Threading;
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
using OpenQA.Selenium;
using WebScraper.Library.Infrastructure;
using WebScraper.Library.Model;
using WebScraper.Library.Sites;
using WebScraper.ViewModel;
using Website.Infrastructure.Configuration;
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
            Bind(listener, "Trace", TraceText, BindingMode.OneWay, TextBox.TextProperty);

            StartDate.SelectedDate = DateTime.Now.Date;
            EndDate.SelectedDate = DateTime.Now.Date;

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        void Bind(object source, string sourceProp, DependencyObject target, BindingMode mode, DependencyProperty dp)
        {
            var binding = new Binding();
            binding.Source = source;
            binding.Path = new PropertyPath(sourceProp);
            binding.Mode = mode;
            BindingOperations.SetBinding(target, dp, binding);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var list = e.Result as List<ImportedFlyerScraperModel>;
            if (list == null)
            {
                UploadEnd(e);
                return;
            }

            var viewList = list.Select(model => new ImportedFlyerScraperViewModel().MapFrom(model)).ToList();
            ImportListView.ItemsSource = viewList;
            NumberValid.DataContext = viewList.Count;
            Load.IsEnabled = true;
            Publish.IsEnabled = true;
        }

        private void UploadEnd(RunWorkerCompletedEventArgs e)
        {
            Load.IsEnabled = true;
            Publish.IsEnabled = true;
            MessageBox.Show("Upload finished");
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var startEnd = e.Argument as StartEnd;
            if (startEnd == null)
            {
                UploadStart(e);
                return;
            }

            var items = new ConcurrentQueue<ImportedFlyerScraperModel>();
            var all = _kernel.GetAll<SiteScraperInterface>(metadata => metadata.Name == DrunkenPoetSiteScraper.BaseUrl).ToList();
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

        private void UploadStart(DoWorkEventArgs e)
        {
            var data = e.Argument as PublishData;
            data.import.ForEach(model =>
            {
                var up = new FlyerUpload(data.authcookie, Guid.Empty.ToString(), model, data.server);
                var ret = up.Request().Result;
            });
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
            Publish.IsEnabled = false;

            Trace.TraceInformation("Starting...");

            worker.RunWorkerAsync(new StartEnd()
                {
                    Start = StartDate.SelectedDate.Value,
                    End = EndDate.SelectedDate.Value
                });

        }

        IWebDriver _driver;
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (_driver == null)
            {
                _driver = _kernel.Get<IWebDriver>();
            }
            
            var config = _kernel.Get<ConfigurationServiceInterface>();
            var server = config.GetSetting("Server");
            _driver.Navigate().GoToUrl(server + "/Account/LoginPage");
            
            while ((Auth = GetAuthCookie()) == null)
            {
                Thread.Sleep(1000);    
            }

            _driver.Navigate().GoToUrl("about:blank");
            _driver.Quit();
            _driver.Dispose();
            _driver = null;
        }

        protected string Auth { get; set; }

        class PublishData
        {
            public string authcookie { get; set; }
            public List<ImportedFlyerScraperModel> import { get; set; }
            public string server { get; set; }
        }
        private void Publish_Click(object sender, RoutedEventArgs e)
        {

            var config = _kernel.Get<ConfigurationServiceInterface>();
            var server = config.GetSetting("Server");

            if (string.IsNullOrWhiteSpace(Auth))
            {
                MessageBox.Show("Login First");
                Login_Click(this, new RoutedEventArgs());
            }

            var items = ImportListView.ItemsSource as IList<ImportedFlyerScraperViewModel>;
            if (items == null || items.Count == 0 || items.Count(i => i.Import) == 0)
            {
                MessageBox.Show("No Flyers to import");
            }

            var import = items.Where(i => i.Import).Select(i => new ImportedFlyerScraperModel().MapFrom(i)).ToList();

            worker.RunWorkerAsync(new PublishData()
                {
                    authcookie = Auth,
                    import = import,
                    server = server                    
                });

        }

        private string GetAuthCookie()
        {
            var cookie = _driver.Manage().Cookies.GetCookieNamed(".ASPXAUTH");
            if (cookie == null)
                return null;
            return cookie.Value;
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
