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
        self.SelectedViewModel = selectedDetailViewModel;
        bf.GigGuideMixin(self);

        var currentBrowser = bf.currentBrowserInstance;

        var currentLocation = new bf.LocationModel(currentBrowser.LastSearchedLocation);
        self.Location = ko.observable(currentLocation);

        self.GetReqUrl = function () {
            return '/webapi/gigs/bydate';
        };

        self.GetReqArgs = function (nextpage) {
            var params = {
                loc: ko.mapping.toJS(self.Location())
            };
            
            return params;
        };

        self.setNoMoreFlyas = function() {
            var nomore = 'No';
            if (self.DateSections().length) {
                nomore += ' more';
            }
            nomore += ' gigs';
            var locality = self.Location().Locality();
            if (locality) {
                nomore += ' around ' + locality;
            }
            
            var date = bf.getDateFromHash() || new Date();
            nomore += ' on ' + date.format("DDD DD MMM");

            nomore += '!';
            
            self.noMoreFliersText(nomore);
        };
        
        self.TryFindLocation = function () {
            bf.getCurrentPosition(function (position, status) {
                if (status === 'OK') {
                    var loc = new bf.LocationModel({ Longitude: position.coords.longitude, Latitude: position.coords.latitude })
                    bf.reverseGeocode(loc, self.Location);
                }
            });
        };
        


        self.Sam = Sammy('#bulletinboard');
        self.SelectedViewModel.addDetailRoutes(self.Sam);
        self.AddGetDateRoute(self.Sam);

        self._Init = function () {

            ko.applyBindings(self);

            self.SelectedViewModel.runSammy(self.Sam);
            //self.TryRequest();

            self.Location.subscribe(function (newValue) {
                self.TryRequest();
            });

        };


    };


})( window, jQuery );
