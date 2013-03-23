(function (window, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['bulletin-board'] = function () {
        
        bf.page = new bf.BulletinBoard(
        new bf.SelectedFlierViewModel(new bf.BehaviourViewModelFactory())
        , new bf.TagsSelector({
            displayInline: true
        })
        , new bf.TileLayoutViewModel('#bulletinboard', new bf.BulletinLayoutProperties()));

        var endscroll = new EndlessScroll(window, {
            fireOnce: false,
            fireDelay: false,
            content: false,
            loader: "",
            bottomPixels: 300,
            insertAfter: "#bulletinboard",
            resetCounter: function (num) {
                bf.page.GetMoreFliers();
                return false;
            }

        });
        endscroll.run();
    };

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
        var currentBrowser = bf.currentBrowserInstance;

        var currentLocation = ko.mapping.fromJS(currentBrowser.LastSearchedLocation, {}, this);
        currentLocation = $.extend(new bf.LocationModel(), currentLocation);

        self.Location = ko.observable(currentLocation);

        self.CreateFlier = ko.observable();

        self.Layout = tileLayout;

        
        
        self.HelpTipPage = 'bulletin';
        self.HelpTipGroups = 'about-posta,bulletin-toolbar,global-toolbar';

        
        self.GetReqUrl = function () {
            return '/api/BulletinApi';
        };

        self.GetReqArgs = function (nextpage) {
            var params = {
                loc: ko.mapping.toJS(self.Location())
                , distance: self.Distance()
                , count: self.initialPagesize
            };

            var len = self.fliers().length;
            if (nextpage && len > 0) 
                params.skipPast = self.fliers()[len - 1].Id;
            
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
            })
            .error(function (jqXhr, textStatus, errorThrown) {
                self.moreFliersPending(false);
                bf.ErrorUtil.HandleRequestError(null, jqXhr, self.ErrorHandler);
            });
        };

        self.SelectedViewModel = selectedDetailViewModel;

        self.getDetailUrl = function (flier) {
            return self.SelectedViewModel.getDetailUrl(flier);
        };

        self.ToggleMap = function () {
            self.ShowMap(!self.ShowMap());       
            self.tagsSelector.ShowTags(false);
        };

        self.ShowTags = function () {
            self.ShowMap(false);
            self.tagsSelector.ShowTags(!self.tagsSelector.ShowTags());
        };

        self.TryRequest = function() {
//            if (self.Location() && self.Location().ValidLocation() && self.Distance() > 0) {
                self.Request();
//            } else if (!self.Location() || !self.Location().ValidLocation()){
//                self.fliers([]);
//                self.moreFliersPending(false);
//                self.noMoreFliers(true);
//            }
        };

        self.TearOff = function (flier) {
            debugger;
            var reqdata = ko.toJSON({
                ClaimEntity: 'Flier',
                EntityId: flier.Id,
                BrowserId: bf.currentBrowserInstance.BrowserId
            });

            $.ajax('/api/claim/', {
                data: reqdata,
                type: "post", contentType: "application/json",
                success: function (result) {
                    self.SelectedViewModel.showDetails(flier);
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    bf.ErrorUtil.HandleRequestError(null, jqXhr);
                }
            });
            return true;
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

            self.TryRequest();

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
