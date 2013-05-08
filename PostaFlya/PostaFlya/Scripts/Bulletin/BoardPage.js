(function (window, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['board-page'] = function () {

        bf.page = new bf.BoardPage(
            new bf.SelectedFlierViewModel(new bf.BehaviourViewModelFactory())
            , new bf.TagsSelector({
                displayInline: true
            })
            , new bf.TileLayoutViewModel('#bulletinboard', new bf.BulletinLayoutProperties())
            , $('#boardid').val()  
        );

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

    bf.BoardPage = function (selectedDetailViewModel
        , tagsSelector
        , tileLayout
        , boardId) {
        var self = this;

        self.tagsSelector = tagsSelector;
        self.initialPagesize = 30;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;

        self.fliers = ko.observableArray([]);

        self.ShowMap = ko.observable(false);
        self.Distance = ko.observable(5);
        var currentBrowser = bf.currentBrowserInstance;

        self.Location = ko.observable(new bf.LocationModel());

        self.CreateFlier = ko.observable();

        self.fliterDate = ko.observable(null);

        self.boardId = boardId;
        self.BoardInfo = ko.observable();

        self.displayDate = ko.computed(function () {
            if (self.fliterDate() == null) {
                return "Latest Fliers";
            }

            return $.datepicker.formatDate('dd M yy', new Date(self.fliterDate()));
        }, this);

        self.Layout = tileLayout;

        self.GetReqUrl = function () {
            return '/api/BulletinApi';
        };

        self.GetReqArgs = function (nextpage) {
            var params = {
                loc: ko.mapping.toJS(self.Location())
                , distance: self.Distance()
                , count: self.initialPagesize
                , board: self.boardId
            };

            var len = self.fliers().length;
            if (nextpage && len > 0)
                params.skipPast = self.fliers()[len - 1].Id;

            var tags = self.tagsSelector.SelectedTags().join();
            if (tags.length > 0)
                params.tags = tags;

            if (self.fliterDate() != null) {
                params.date = new Date(self.fliterDate()).toISOString();
            }

            return params;
        };

        self.Request = function () {

            if (self.moreFliersPending())
                return;

            self.fliers([]);
            self.moreFliersPending(true);
            self.noMoreFliersText('');

            $.getJSON(self.GetReqUrl(), self.GetReqArgs(false), function (allData) {

                self.fliers(allData);
                if (allData.length == 0)
                    self.setNoMoreFlyas();
            }).always(function () {
                self.moreFliersPending(false);
            });
        };

        self.setDate = function (date) {
            self.fliterDate(date);
        };

        self.TryFindLocation = function () {
            bf.getCurrentPosition(function (position, status) {
                if (status === 'OK') {
                    var loc = new bf.LocationModel({ Longitude: position.coords.longitude, Latitude: position.coords.latitude })
                    bf.reverseGeocode(loc, self.Location);
                }
            });
        };

        self.moreFliersPending = ko.observable(false);
        self.noMoreFliersText = ko.observable('');

        self.GetMoreFliers = function () {
            if (self.moreFliersPending() || self.noMoreFliersText())
                return;
            self.moreFliersPending(true);
            $.getJSON(self.GetReqUrl(), self.GetReqArgs(true), function (allData) {
                self.fliers.pushAll(allData);
                if (allData.length == 0)
                    self.setNoMoreFlyas();
            })
            .fail(function (jqXhr, textStatus, errorThrown) {
                bf.ErrorUtil.HandleRequestError(null, jqXhr, self.ErrorHandler);
            })
            .always(function () {
                self.moreFliersPending(false);
            });;
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

        self.TryRequest = function () {
            self.Request();
        };

        self.setNoMoreFlyas = function () {
            var nomore = 'No';
            if (self.fliers().length) {
                nomore += ' more';
            }
            nomore += ' happenings';
            var locality = self.Location().Locality();
            if (locality) {
                nomore += ' around ' + locality;
            }
            nomore += '! Why not ';

            self.noMoreFliersText(nomore);

        };

        self.StatusText = ko.computed(function () {

            if (self.moreFliersPending() || (self.noMoreFliersText() && self.fliers().length == 0))
                return '';

            var showingmostrecent = 'Showing most recent posts';

            if (self.fliterDate() != null) {
                showingmostrecent = "Showing events on " + new Date(self.fliterDate()).toDateString();
            }

            var validLoc = self.Location().ValidLocation();
            var locality = self.Location().Locality();
            if (!validLoc)
                return showingmostrecent;

            showingmostrecent += ' within ' + self.Distance() + 'km of';

            if (locality)
                return showingmostrecent + ' ' + locality;

            return showingmostrecent + ' you';

        }, self);

        self.TearOff = function (flier) {

            bf.pagedefaultaction.set('bulletin-detail', 'peel');
            self.SelectedViewModel.showDetails(flier);
            return true;
        };

        self.Sam = Sammy('#bulletinboard');
        self.SelectedViewModel.addDetailRoutes(self.Sam);

        self._Init = function () {

            self.SelectedViewModel.runSammy(self.Sam);
            
            if (bf.pageState !== undefined && bf.pageState.Fliers !== undefined) {

                self.fliers([]);
                self.fliers(bf.pageState.Fliers);
            }

//            if (bf.pageState !== undefined && bf.pageState.BoardInfo !== undefined) {
//                self.BoardInfo([]);
//            }

            ko.applyBindings(self);

            self.tagsSelector.updateCallback = self.TryRequest;

            self.tagsSelector.LoadTags();


            self.TryRequest();

            self.Location.subscribe(function (newValue) {
                self.TryRequest();
            });
            self.Distance.subscribe(function (newValue) {
                self.TryRequest();
            });

            self.fliterDate.subscribe(function (newValue) {
                self.TryRequest();
            });

        };

        self._Init();
    };

})(window);
