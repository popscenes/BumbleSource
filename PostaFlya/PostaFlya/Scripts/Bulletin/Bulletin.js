(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.BulletinBoard = function (selectedDetailViewModel
        , tagsSelector
        , tileLayout) {
        var self = this;

        self.tagsSelector = tagsSelector;
        self.initialPagesize = 30;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;

        self.fliers = ko.observableArray([]);

        self.ShowMap = ko.observable(false);
        self.Distance = ko.observable(5);
        self.Location = ko.observable(new bf.LocationModel());

        self.CreateFlier = ko.observable();

        self.Layout = tileLayout;

        
        self.GetReqUrl = function () {
            return '/api/BulletinApi';
        };

        self.GetReqArgs = function (nextpage) {
            var params = {
                loc: self.Location().LatLong()
                , distance: self.Distance()
                , count: self.initialPagesize
            };

            var len = self.fliers().length;
            if (nextpage && len > 0) 
                params.skip = len;
            
            var tags = self.tagsSelector.SelectedTags().join();
            if (tags.length > 0)
                params.tags = tags;

            return params;
        };

        self.hideShowAbout = ko.observable(false);

        self.ShowAbout = function() {
            self.hideShowAbout(!self.hideShowAbout());
            self.tagsSelector.ShowTags(false);
        };

        self.Request = function () {
            self.noMoreFliers(false);

            $.getJSON(self.GetReqUrl(), self.GetReqArgs(false), function (allData) {
                self.fliers([]);
                self.fliers(allData);
            });
        };

        self.TryFindLocation = function() {
            bf.getCurrentPosition(function (position, status) {
                if (status === 'OK') {
                    self.Location(
                        new bf.LocationModel({ Longitude: position.coords.longitude, Latitude: position.coords.latitude })
                    );                    
                }
            });
        };

        self.moreFliersPending = ko.observable(false);
        self.noMoreFliers = ko.observable(false);

        self.GetMoreFliers = function () {
            if (self.moreFliersPending() || self.noMoreFliers())
                return;
            self.moreFliersPending(true);
            $.getJSON(self.GetReqUrl(), self.GetReqArgs(true), function (allData) {
                self.fliers.pushAll(allData);
                if (allData.length == 0)
                    self.noMoreFliers(true);
                self.moreFliersPending(false);
            });
        };

        self.SelectedViewModel = selectedDetailViewModel;

        self.getDetailUrl = function (flier) {
            return self.SelectedViewModel.getDetailUrl(flier);
        };

        self.ToggleMap = function () {
            self.ShowMap(!self.ShowMap());
           
            self.tagsSelector.ShowTags(false);
            self.hideShowAbout(false);
        };

        self.ShowTags = function () {
            self.ShowMap(false);
            self.tagsSelector.ShowTags(!self.tagsSelector.ShowTags());
            self.hideShowAbout(false);
        };

        self.TryRequest = function() {
            if (self.Location() && self.Location().ValidLocation() && self.Distance() > 0) {
                self.Request();
            } else if (!self.Location() || !self.Location().ValidLocation()){
                self.fliers([]);
            }
        };

        self.Sam = Sammy('#bulletinboard');
        self.SelectedViewModel.addDetailRoutes(self.Sam);

        self._Init = function () {

            self.SelectedViewModel.runSammy(self.Sam);

            ko.applyBindings(self);

            self.tagsSelector.updateCallback = self.TryRequest;

            self.tagsSelector.LoadTags();


            if (bf.pageState !== undefined && bf.pageState.Fliers !== undefined) {
                
                self.fliers([]);
                self.fliers(bf.pageState.Fliers);
            }

            self.TryFindLocation();

            self.Location.subscribe(function (newValue) {
                self.TryRequest();
            });
            self.Distance.subscribe(function (newValue) {
                self.TryRequest();
            });

        };

        self._Init();
    };

})(window);
