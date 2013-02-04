(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.BulletinBoard = function (locationSelector
        , selectedDetailViewModel
        , tagsSelector
        , tileLayout) {
        var self = this;

        self.locationSelector = locationSelector;
        //self.distanceSelector = distanceSelector;
        self.tagsSelector = tagsSelector;
        self.initialPagesize = 30;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;

        self.fliers = ko.observableArray([]);

        self.CreateFlier = ko.observable();

        self.Layout = tileLayout;

        self.GetReqUrl = function () {
            return '/api/BulletinApi';
        };

        self.GetReqArgs = function (nextpage) {
            var params = {
                loc: self.locationSelector.currentLocation().LatLong()
                , distance: self.locationSelector.currentDistance()
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

        self.hideShowAbout = ko.observable(true);

        self.ShowAbout = function() {
            self.hideShowAbout(!self.hideShowAbout());
            self.locationSelector.showMain(false);
            self.tagsSelector.ShowTags(false);
        };

        self.Request = function () {
            self.noMoreFliers(false);

            $.getJSON(self.GetReqUrl(), self.GetReqArgs(false), function (allData) {
                self.fliers([]);
                self.fliers(allData);
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

        self.LocationAndDistanceCallback = function () {
            if (self.locationSelector.ValidLocation())
                self.Request();
        };

        self.SelectedViewModel = selectedDetailViewModel;

        self.getDetailUrl = function (flier) {
            return self.SelectedViewModel.getDetailUrl(flier);
        };

        self.ToggleMap = function () {
            self.locationSelector.toggleShowMain();
            self.tagsSelector.ShowTags(false);
            self.hideShowAbout(false);
        };

        self.ShowTags = function () {
            self.locationSelector.showMain(false);
            self.tagsSelector.ShowTags(!self.tagsSelector.ShowTags());
            self.hideShowAbout(false);
        };

        self.Sam = Sammy('#bulletinboard');
        self.SelectedViewModel.addDetailRoutes(self.Sam);

        self._Init = function () {
            self.locationSelector.showMain(false);
            self.SelectedViewModel.runSammy(self.Sam);

            ko.applyBindings(self);
            self.locationSelector.updateCallback = self.LocationAndDistanceCallback;
            //self.distanceSelector.updateCallback = self.LocationAndDistanceCallback;
            self.tagsSelector.updateCallback = self.LocationAndDistanceCallback;

            self.tagsSelector.LoadTags();
            self.locationSelector.SetCurrentlocationFromGeoCode();

            if (bf.pageState !== undefined && bf.pageState.Fliers !== undefined) {
                
                self.fliers([]);
                self.fliers(bf.pageState.Fliers);
            }

            self.locationSelector.ShowMap();

        };

        self._Init();
    };

})(window);
