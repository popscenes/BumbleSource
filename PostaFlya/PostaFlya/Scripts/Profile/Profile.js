(function (window, undefined) {

    var bf = window.bf = window.bf || {};

    bf.ProfileViewModel = function (selectedDetailViewModel, myFliersLayout, likedFliersLayout) {
        var self = this;

        self.SelectedViewModel = selectedDetailViewModel;
        self.MyFliersLayout = myFliersLayout;
        self.Layout = self.MyFliersLayout;

        self.LikedFliersLayout = likedFliersLayout;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;


        self.MyFliers = ko.observableArray([]);
        self.LikedFliers = ko.observableArray([]);

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
                self.LikedFliers([]);
                self.MyFliers(bf.pageState.ProfileModel.Fliers);
                self.LikedFliers(bf.pageState.ProfileModel.LikedFliers);
            } else {
                $.getJSON('/api/Profile/' + self.GetHandleForReq(), function (allData) {
                    self.MyFliers([]);
                    self.LikedFliers([]);
                    self.MyFliers(allData.Fliers);
                    self.LikedFliers(allData.LikedFliers);
                });
            }

        };

        self._Init();

    };


})(window);