(function (window, $, undefined) {

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
        
        self.GetReqUrl = function () {
            return "/webapi/peeled/" + bf.currentBrowserInstance.BrowserId + "/gigs";

        };

        self.GetReqArgs = function () {
            var params = { };
            return params;
        };
        
        bf.GigGuideMixin(self);



        self.Sam = Sammy('#profile-content');
        self.SelectedViewModel.addDetailRoutes(self.Sam);

        self._Init = function () {
            self.Sam.run(self.SelectedViewModel.initPath);
            ko.applyBindings(self);
            self.TryRequest();
        };

        self._Init();

    };


})(window, jQuery);