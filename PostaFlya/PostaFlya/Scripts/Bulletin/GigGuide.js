(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['gig-guide'] = function () {
        
        bf.page = new bf.GigGuide(
        new bf.SelectedFlierViewModel(new bf.BehaviourViewModelFactory()));
        bf.page._Init();

    };

    bf.GigGuide = function (selectedDetailViewModel) {
        var self = this;

        self.initialPagesize = 30;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;


        var currentBrowser = bf.currentBrowserInstance;

        var currentLocation = new bf.LocationModel(currentBrowser.LastSearchedLocation);
        self.Location = ko.observable(currentLocation);
        self.StartDate = ko.observable(null);

        self.CreateFlier = ko.observable();
        
        self.DateSections = ko.observableArray([]);
        
        //mobileapi/gigs/bydate?lat=-37.769&lng=144.979&distance=10&start=2013-06-29&end=2013-07-02
        self.GetReqUrl = function () {
            return '/mobileapi/gigs/bydate';
        };

        self.GetReqArgs = function (nextpage) {
            var loc = ko.mapping.toJS(self.Location());
            var params = {
                lat: loc.Latitude,
                lng: loc.Longitude        
            };
            
            if (self.StartDate() != null) {
                params.Start = self.StartDate();
            }

            return params;
        };
        
        function getImageExt(image, width, axis) {
            var dd = image.Extensions[0];
            for (var d = 0 ; d < image.Extensions.length; d++) {
                var ext = image.Extensions[d];
                if (ext.Width == width && ext.ScaleAxis == axis) {
                    dd = ext;
                    break;    
                }
            }
            return dd;
        }

        self.Request = function () {

            if (self.moreFliersPending() || !self.Location().ValidLocation())
                return;

            self.moreFliersPending(true);
            self.noMoreFliersText('');
            
            $.ajax(
                 {
                     dataType: (bf.widgetbase ? "jsonp" : "json"),
                     url: self.GetReqUrl(),
                     crossDomain: (bf.widgetbase ? true : false),
                     data: self.GetReqArgs(false)
                 }
             ).done(function (resp) {

                 var allData = resp.Data;
                 for (var i = 0; i < allData.Dates.length; i++) {
                     var next = allData.Dates[i];
                     next.Flyers = [];
                     for (var f = 0; f < next.FlyerIds.length; f++) {
                         var flyer = allData.Flyers[next.FlyerIds[f]];
                         var dd = getImageExt(flyer.Image, 150, 'Square');

                         flyer.Image.FlierImageUrl = flyer.Image.BaseUrl + dd.UrlExtension;
                         flyer.Image.Width = dd.Width;
                         flyer.Image.Height = dd.Height;
                         next.Flyers.push(flyer);
                     }

                     next.Date = new Date(next.Date);
                     next.DateLink = next.Date.format("DDMMMYYYY");
                     next.Dates = [];
                     for (var d = -2; d <= 2; d++) {
                         var pickDate = new Date(next.Date);
                         pickDate.setDate(pickDate.getDate() + d);
                         next.Dates.push({
                             Datestring: pickDate.format("DDD DD MMM"),
                             DateLink: '#' + pickDate.format("DDMMMYYYY"),
                             Date: pickDate,
                             Ishistory: i < 0,
                             Iscurrent: i == 0,
                             Isfuture: i > 0
                         });
                     }

                     self.DateSections.push(next);
                 }
                 
                 if (allData.Dates.length == 0)
                     self.setNoMoreFlyas();

             }).always(function () {
                 self.moreFliersPending(false);
             });

        };

        self.navigateToDate = function(dateData) {
            if ($(dateData.DateLink).length > 0)
                return true;

            self.DateSections.removeAll();
            self.StartDate(dataData.Date);

        };

        self.TryFindLocation = function() {
            bf.getCurrentPosition(function (position, status) {
                if (status === 'OK') {
                    var loc = new bf.LocationModel({ Longitude: position.coords.longitude, Latitude: position.coords.latitude })
                    bf.reverseGeocode(loc, self.Location);                   
                }
            });
        };

        self.moreFliersPending = ko.observable(false);
        self.noMoreFliersText = ko.observable('');


        self.SelectedViewModel = selectedDetailViewModel;

        self.getDetailUrl = function (flier) {
            return self.SelectedViewModel.getDetailUrl(flier);
        };

        self.TryRequest = function() {
                self.Request();
        };

        self.setNoMoreFlyas = function() {
            var nomore = 'No';
            if (self.fliers().length) {
                nomore += ' more';
            }
            nomore += ' gigs';
            var locality = self.Location().Locality();
            if (locality) {
                nomore += ' around ' + locality;
            }
            nomore += '!';
            
            self.noMoreFliersText(nomore);

        };
        
        bf.dateFilter(self, self.fliterDate);

        self.StatusText = ko.computed(function () {           
            
            if (self.moreFliersPending() || (self.noMoreFliersText() && self.fliers().length == 0))
                return '';


            var showingmostrecent = "Showing gigs ";

            return showingmostrecent;

        }, self);


        self.TearOff = function (flier) {

            bf.pagedefaultaction.set('bulletin-detail', 'peel');
            self.SelectedViewModel.showDetails(flier);
            return true;
        };
        
        self.FlierTemplate = function (flier) {
            var isOwner = (bf.currentBrowserInstance && flier.BrowserId == bf.currentBrowserInstance.BrowserId);
            var ret = 'BehaviourDefault-template';
            return isOwner ? ret + '-owner' : ret;
        };

        self.Sam = Sammy('#bulletinboard');
        self.SelectedViewModel.addDetailRoutes(self.Sam);

        self._Init = function () {

            self.SelectedViewModel.runSammy(self.Sam);

            ko.applyBindings(self);

            self.TryRequest();

            self.Location.subscribe(function (newValue) {
                self.TryRequest();
            });

        };


    };


})( window, jQuery );
