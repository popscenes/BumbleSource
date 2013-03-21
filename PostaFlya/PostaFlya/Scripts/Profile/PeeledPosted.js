(function (window, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['profile-posted'] = function () {
        bf.page = new bf.ProfileFlyasViewModel(new bf.SelectedFlierViewModel(new bf.BehaviourViewModelFactory(), location.pathname)
        , new bf.TileLayoutViewModel('#profile-fliers', new bf.BulletinLayoutProperties())
        , 'myfliers');
    };
    bf.pageinit['profile-peeled'] = function () {
        bf.page = new bf.ProfileFlyasViewModel(new bf.SelectedFlierViewModel(new bf.BehaviourViewModelFactory(), location.pathname)
        , new bf.TileLayoutViewModel('#profile-fliers', new bf.BulletinLayoutProperties())
        , 'claim');
    };

    bf.ProfileFlyasViewModel = function (selectedDetailViewModel, flyaLayout, action) {
        var self = this;

        self.SelectedViewModel = selectedDetailViewModel;
        self.FliersLayout = flyaLayout;
        self.Layout = self.FliersLayout;

        self.Action = action;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;

        self.Fliers = ko.observableArray([]);

        self.getDetailUrl = function (flier) {
            return self.SelectedViewModel.getDetailUrl(flier);
        };

        self.Sam = Sammy('#profile-content');
        self.SelectedViewModel.addDetailRoutes(self.Sam);

        self._Init = function () {
            self.Sam.run(self.SelectedViewModel.initPath);
            ko.applyBindings(self);

            if (bf.pageState !== undefined && bf.pageState.Fliers !== undefined) {
                self.Fliers([]);
                self.Fliers(bf.pageState.Fliers);
            } else {
                $.getJSON('/api/Browser/' + bf.currentBrowserInstance.BrowserId + '/' + self.Action, function (allData) {
                    self.Fliers([]);
                    self.Fliers(allData);
                })
                .error(function(jqXhr, textStatus, errorThrown) {
                    bf.ErrorUtil.HandleRequestError(null, jqXhr, self.ErrorHandler);
                });
            }

        };

        self._Init();

    };


})(window);