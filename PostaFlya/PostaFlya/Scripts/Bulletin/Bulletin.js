(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.BulletinBoard = function (locationSelector
        , distanceSelector
        , selectedDetailViewModel
        , tagsSelector
        , tileLayout) {
        var self = this;

        self.locationSelector = locationSelector;
        self.distanceSelector = distanceSelector;
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
            var params = { loc: { Latitude: self.locationSelector.latitude(), Longitude: self.locationSelector.longitude() }
                , distance: self.distanceSelector.currentDistance()
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

        self.InvalidLocation = ko.computed(function () {
            return !self.locationSelector.ValidLocation();
        }, self);

        self.LocationAndDistanceCallback = function () {
            if (!self.InvalidLocation())
                self.Request();
        };

        self.SelectedViewModel = selectedDetailViewModel;

        self.getDetailUrl = function (flier) {
            return self.SelectedViewModel.getDetailUrl(flier);
        };

        self.Sam = Sammy('#bulletinboard');
        self.SelectedViewModel.addDetailRoutes(self.Sam);

        self._Init = function () {
            self.SelectedViewModel.runSammy(self.Sam);

            ko.applyBindings(self);
            self.locationSelector.updateCallback = self.LocationAndDistanceCallback;
            self.distanceSelector.updateCallback = self.LocationAndDistanceCallback;
            self.tagsSelector.updateCallback = self.LocationAndDistanceCallback;

            self.tagsSelector.LoadTags();

            if (bf.pageState !== undefined && bf.pageState.Fliers !== undefined) {
                
                self.fliers([]);
                self.fliers(bf.pageState.Fliers);
            }

        };

        self._Init();
    };

})(window);
//