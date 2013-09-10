using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Venue;
using PostaFlya.Models.Board;
using PostaFlya.Models.Location;
using WebScraper.Library.Infrastructure;
using WebScraper.Library.Model;
using WebScraper.Library.Sites;
using WebScraper.ViewModel;
using Website.Application.Google.Places.Details;
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
        public BoardCreateEditModel _board { get; set; }
        
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private readonly BackgroundWorker googlePlacesWorker = new BackgroundWorker();
        private readonly BackgroundWorker boardUpdateWorker = new BackgroundWorker();
        private readonly BackgroundWorker ImageUploadWorker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            _kernel = new StandardKernel();
            _kernel.Load<RegisterSites>();
            var listener = new MyTraceListener();
            Trace.Listeners.Add(listener);

            TraceText.DataContext = listener;
            Bind(listener, "Trace", TraceText, BindingMode.OneWay, TextBox.TextProperty);

            var all = _kernel.GetBindings(typeof (SiteScraperInterface));
            var venues = all.Select(binding => binding.Metadata.Name).ToList();
            venues.Insert(0, "<All>");
            VenueCombo.ItemsSource = venues;
            VenueCombo.SelectedItem = venues.First();

           // PlaceName.DataContext = _venueModel;
            //Bind(_venueModel, "PlaceName", PlaceName, BindingMode.TwoWay, TextBox.TextProperty);

            StartDate.SelectedDate = DateTime.Now.Date;
            EndDate.SelectedDate = DateTime.Now.Date;

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            googlePlacesWorker.DoWork += googlePlacesWorker_DoWork;
            googlePlacesWorker.RunWorkerCompleted += googlePlacesWorker_RunWorkerCompleted;

            boardUpdateWorker.DoWork += boardUpdateWorker_DoWork;
            boardUpdateWorker.RunWorkerCompleted += boardUpdateWorker_RunWorkerCompleted;

            autoBoardAdmins_GotFocus(this, new RoutedEventArgs());

            ImageUploadWorker.DoWork += ImageUploadWorker_DoWork;
            ImageUploadWorker.RunWorkerCompleted += ImageUploadWorker_RunWorkerCompleted;        
        }

        void ImageUploadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var imageId = e.Result as String;
            if (imageId != "")
            {
                Trace.TraceInformation("Image Uploaded");
                ImageId.Text = imageId;
            }
        }

        void ImageUploadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var data = e.Argument as ImnageUploadData;
            var uploader = new ImageUpload(data.authcookie, data.server, data.Image, data.ImageName);
            var ret = uploader.UploadImage().Result;
            e.Result = ret;
        }

        void boardUpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var success = e.Result as bool?;
            if (success != null && success.Value)
            {
                Trace.TraceInformation("Board Uploaded");
            }
        }

        void boardUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var data = e.Argument as BoardPublishData;
            var uploader = new BoardUpload(data.authcookie, Guid.Empty.ToString(), data.board, data.server);
            var ret = uploader.Request().Result;
            e.Result = ret;

        }

        void googlePlacesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var googlePLacesRef = e.Argument as String;
            var locModel = Util.GetVenueInformationModelFromGooglePleacesRef(googlePLacesRef);
            e.Result = locModel;
        }

        void googlePlacesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var venueInfo = e.Result as VenueInformationModel;
            _board = new BoardCreateEditModel()
                {
                    VenueInformation = venueInfo,
                    BoardName = venueInfo.PlaceName,
                    AllowOthersToPostFliers = false,
                    Description = venueInfo.PlaceName + " Venue Board",
                    RequireApprovalOfPostedFliers = false,
                    Status = BoardStatus.Approved,
                    TypeOfBoard = BoardTypeEnum.VenueBoard
                };

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
            //var all = _kernel.GetAll<SiteScraperInterface>(metadata => metadata.Name == DrunkenPoetSiteScraper.BaseUrl).ToList();
            var all = new List<SiteScraperInterface>();
            if("<All>" == startEnd.SelectedVenue)
                all.AddRange(_kernel.GetAll<SiteScraperInterface>());
            else
                all.Add(_kernel.Get<SiteScraperInterface>(metadata => metadata.Name == startEnd.SelectedVenue));
            
            Parallel.ForEach(all, siteScraper =>
                {
                    Trace.TraceInformation("Running " + siteScraper.SiteName);
                    var next = siteScraper.GetFlyersFrom(startEnd.Start, startEnd.End);
                    foreach (
                        var importedFlyerScraperModel in
                            next.Where(model => model.EventDates.All(time => time <= startEnd.End && time >= startEnd.Start)))
                    {
                        items.Enqueue(importedFlyerScraperModel);
                    }

                });

            e.Result = items.ToList();
        }

        private void UploadStart(DoWorkEventArgs e)
        {
            var data = e.Argument as PublishData;
            var boardsReq = new BoardsGet(data.server, data.authcookie);
            
            data.import.ForEach(model =>
                {
                    var boardId = "";
                    var allBoards = boardsReq.Request().Result;
                    var foundBoards = allBoards.Where(_ => _.DefaultVenueInformation.PlaceName == model.VenueInfo.PlaceName).Select(_ => _.Id);

                    if (allBoards.All(_ => _.DefaultVenueInformation.PlaceName != model.VenueInfo.PlaceName))
                    {
                        Trace.TraceInformation("Creating board for gig no board found for {0} ...", model.VenueInfo.PlaceName);

                        var venueInfo = model.VenueInfo;
                        var newBoard = new BoardCreateEditModel()
                        {
                            VenueInformation = venueInfo,
                            BoardName = venueInfo.PlaceName,
                            AllowOthersToPostFliers = false,
                            Description = venueInfo.PlaceName + " Venue Board",
                            RequireApprovalOfPostedFliers = false,
                            Status = BoardStatus.Approved,
                            TypeOfBoard = BoardTypeEnum.VenueBoard,
                            AdminEmailAddresses = data.boardAdmins
                        };

                        var uploadBoardReq = new BoardUpload(data.authcookie, Guid.Empty.ToString(), newBoard, data.server);
                            
                        boardId = uploadBoardReq.Request().Result;
                        Trace.TraceInformation("Created new board {0} Id {1}", newBoard.BoardName, boardId);

                    }
                    else
                    {
                        boardId = allBoards.First(_ => _.DefaultVenueInformation.PlaceName == model.VenueInfo.PlaceName).Id;
                    }


                    if (!String.IsNullOrWhiteSpace(boardId))
                    {
                        model.BoardId = boardId;
                        var up = new FlyerUpload(data.authcookie, Guid.Empty.ToString(), model, data.server);
                        var ret = up.Request().Result;
                    }
                    else
                    {
                        Trace.TraceInformation("no baord found or created for {0}", model.VenueInfo.PlaceName);
                    }

                });
            
        }

        private class StartEnd
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public string SelectedVenue { get; set; }
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
                    End = EndDate.SelectedDate.Value,
                    SelectedVenue = VenueCombo.SelectedItem.ToString()
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
            public List<String> boardAdmins { get; set; }
        }

        class BoardPublishData
        {
            public string authcookie { get; set; }
            public BoardCreateEditModel board { get; set; }
            public string server { get; set; }
        }

        class ImnageUploadData
        {
            public string authcookie { get; set; }
            public Image Image { get; set; }
            public String ImageName { get; set; }
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

            var import = items.Where(i => i.Import)
                .Distinct(new ImportedFlyerScraperViewModelEqual())
                .Select(i => new ImportedFlyerScraperModel().MapFrom(i)).ToList();

            worker.RunWorkerAsync(new PublishData()
                {
                    authcookie = Auth,
                    import = import,
                    server = server,
                    boardAdmins = autoBoardAdmins.Text.Split(new char[]{','}).ToList()
                });

        }

        private string GetAuthCookie()
        {
            var cookie = _driver.Manage().Cookies.GetCookieNamed(".ASPXAUTH");
            if (cookie == null)
                return null;
            return cookie.Value;
        }

        private void Places_OnClick(object sender, RoutedEventArgs e)
        {
            googlePlacesWorker.RunWorkerAsync(GooglePlacesRef.Text);
        }


        private void UploadBoard_OnClick(object sender, RoutedEventArgs e)
        {
            var config = _kernel.Get<ConfigurationServiceInterface>();
            var server = config.GetSetting("Server");

            if (string.IsNullOrWhiteSpace(Auth))
            {
                MessageBox.Show("Login First");
                Login_Click(this, new RoutedEventArgs());
            }


            if (_board == null)
            {
                MessageBox.Show("No board to import");
                return;
            }

            _board.Description = Description.Text;
            _board.AdminEmailAddresses = AdminList.Text.Split(new char[] {','}).ToList();

            boardUpdateWorker.RunWorkerAsync(new BoardPublishData()
            {
                authcookie = Auth,
                board = _board,
                server = server
            });

        }

        private void autoBoardAdmins_GotFocus(object sender, RoutedEventArgs e)
        {
            if (autoBoardAdmins.Text.Equals("Board Auto-Create Admins"))
                autoBoardAdmins.Text = "teddymccuddles@gmail.com, rickyaudsley@gmail.com";

            autoBoardAdmins.SelectAll();
        }

        private void Image_Browse_click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".jpg"; // Default file extension
            dlg.Filter = "Jpeg (.jpg)|*.jpg| Pngs | *.png"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document
                ImagePath.Content = dlg.FileName;
                ImageUpload.IsEnabled = true;

                
                
            }
        }

        private void Image_Upload_click(object sender, RoutedEventArgs e)
        {
            Image image = new Bitmap(ImagePath.Content as string);
            var config = _kernel.Get<ConfigurationServiceInterface>();
            var server = config.GetSetting("Server");
            
            ImageUploadWorker.RunWorkerAsync(new ImnageUploadData()
            {
                authcookie = Auth,
                Image = image,
                ImageName = ImagePath.Content as String,
                server = server
            });
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
