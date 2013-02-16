(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.ProfileViewModel = function (selectedDetailViewModel, myFliersLayout, claimedFliersLayout) {
        var self = this;

        self.SelectedViewModel = selectedDetailViewModel;
        self.MyFliersLayout = myFliersLayout;
        self.Layout = self.MyFliersLayout;

        self.ClaimedFliersLayout = claimedFliersLayout;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;


        self.MyFliers = ko.observableArray([]);
        self.ClaimedFliers = ko.observableArray([]);

        self.getDetailUrl = function (flier) {
            return self.SelectedViewModel.getDetailUrl(flier);
        };

        self.GetHandleForReq = function () {
            if (location.pathname.indexOf('/Profile/View') >= 0)
                return '/' + bf.currentBrowserInstance.Handle;
            return location.pathname.split('/')[1];
        };


        self.Sam = Sammy('#profile-view');
        self.SelectedViewModel.addDetailRoutes(self.Sam);

        self._Init = function () {
            self.Sam.run(self.SelectedViewModel.initPath);
            ko.applyBindings(self);

            if (bf.pageState !== undefined && bf.pageState.ProfileModel !== undefined) {
                self.MyFliers([]);
                self.ClaimedFliers([]);
                self.MyFliers(bf.pageState.ProfileModel.Fliers);
                self.ClaimedFliers(bf.pageState.ProfileModel.ClaimedFliers);
            } else {
                $.getJSON('/api/Profile/' + self.GetHandleForReq(), function (allData) {
                    self.MyFliers([]);
                    self.ClaimedFliers([]);
                    self.MyFliers(allData.Fliers);
                    self.ClaimedFliers(allData.ClaimedFliers);
                })
                .error(function(jqXhr, textStatus, errorThrown) {
                    bf.ErrorUtil.HandleRequestError(null, jqXhr, self.ErrorHandler);
                });
            }

        };

        self._Init();

    };


})(window);