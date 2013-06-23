(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};

    var pageInitInternal = function(tryFindAuto) {
        bf.page = new bf.BoardSearchBoard(new bf.TileLayoutViewModel('#bulletinboard', new bf.BulletinLayoutProperties()), tryFindAuto);

        var endscroll = new EndlessScroll(window, {
            fireOnce: false,
            fireDelay: false,
            content: false,
            loader: "",
            bottomPixels: 300,
            insertAfter: "#bulletinboard",
            resetCounter: function (num) {
                bf.page.GetMoreBoards();
                return false;
            }

        });
        endscroll.run();
    };
    
    bf.pageinit['board-search-page'] = function () {
        pageInitInternal(false);
    };
    bf.pageinit['board-search-page-auto'] = function () {
        pageInitInternal(true);
    };

    bf.BoardSearchBoard = function (tileLayout, tryFindAuto) {
        var self = this;

        self.initialPagesize = 30;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;
        self.tryFindAuto = tryFindAuto;

        self.boards = ko.observableArray([]);

        self.Distance = ko.observable(5);
        var currentBrowser = bf.currentBrowserInstance;

        var currentLocation = new bf.LocationModel(currentBrowser.LastSearchedLocation);
        self.Location = ko.observable(currentLocation);

        self.Layout = tileLayout;

        self.GetReqUrl = function () {
            return '/api/BoardSearchApi';
        };

        self.GetReqArgs = function (nextpage) {
            var params = {
                loc: ko.mapping.toJS(self.Location())
                , distance: self.Distance() * 1000
                , count: self.initialPagesize
            };

            var len = self.boards().length;
            if (nextpage && len > 0)
                params.skip = len;
            
            return params;
        };
        
        var mapping = {
            'Location': {
                create: function (options) {
                    return ko.observable(new bf.LocationModel(options.data));
                }
            },
            'DefaultVenueInformation': {
                create: function (options) {
                    return ko.observable(new bf.VenueInformationModel(options.data));
                }
            }
        };

        self.Request = function () {

            if (self.moreBoardsPending())
                return;

            self.boards([]);
            self.moreBoardsPending(true);
            self.noMoreBoardsText('');

            $.getJSON(self.GetReqUrl(), self.GetReqArgs(false), function (allData) {

                $.each(allData, function (index, value) {
                    value.Location = new bf.LocationModel(value.Location);
                });
                self.boards(allData);
                if (allData.length == 0)
                    self.setNoMoreBoards();
            }).always(function () {
                self.moreBoardsPending(false);
            });
        };

        self.TryingToFindLocation = ko.observable(false);
        self.TryFindLocation = function () {
            self.TryingToFindLocation(true);
            bf.getCurrentPosition(function (position, status) {
                if (status === 'OK') {
                    var loc = new bf.LocationModel({ Longitude: position.coords.longitude, Latitude: position.coords.latitude });
                    bf.reverseGeocode(loc, self.Location);
                }
                self.TryingToFindLocation(false);
            });
        };

        self.moreBoardsPending = ko.observable(false);
        self.noMoreBoardsText = ko.observable('');

        self.GetMoreBoards = function () {
            if (self.moreBoardsPending() || self.noMoreBoardsText())
                return;
            self.moreBoardsPending(true);
            $.getJSON(self.GetReqUrl(), self.GetReqArgs(true), function (allData) {
                $.each(allData, function (index, value) {
                    value.Location = new bf.LocationModel(value.Location);
                });
                self.boards.pushAll(arr);
                if (allData.length == 0)
                    self.setNoMoreBoards();
            })
            .fail(function (jqXhr, textStatus, errorThrown) {
                bf.ErrorUtil.HandleRequestError(null, jqXhr, self.ErrorHandler);
            })
            .always(function () {
                self.moreBoardsPending(false);
            });;
        };

        self.TryRequest = function () {
            self.Request();
        };

        self.setNoMoreBoards = function () {
            var nomore = 'No';
            if (self.boards().length) {
                nomore += ' more';
            }
            nomore += ' venue or festival boards ';
            var locality = self.Location().Locality();
            if (locality) {
                nomore += ' around ' + locality;
            }

            self.noMoreBoardsText(nomore);

        };


        self.StatusText = ko.computed(function () {

            if (self.moreBoardsPending() || (self.noMoreBoardsText() && self.boards().length == 0))
                return '';

            var showingmostrecent = 'Showing boards or festivals ';

            var validLoc = self.Location().ValidLocation();
            var locality = self.Location().Locality();
            if (!validLoc)
                return showingmostrecent;

            showingmostrecent += ' within ' + self.Distance() + 'km of';

            if (locality)
                return showingmostrecent + ' ' + locality;

            return showingmostrecent + ' you';

        }, self);

        self._Init = function () {


            ko.applyBindings(self);

            if (self.tryFindAuto)
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


})(window, jQuery);
