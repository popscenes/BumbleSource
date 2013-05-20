
(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['bulletin-detail'] = function () {
        bf.page = new bf.BehaviourViewModel(new bf.BehaviourViewModelFactory(), location.pathname);
//        bf.locforview = new bf.LocationService(false);
//        bf.locforview.Start(function (ls) {
//            var reqdata = { id: location.pathname };
//            $.extend(reqdata, ls.currentLocation);
//            $.get("/TrackView/Loc", reqdata);
//        });
    };
    
})(window, jQuery);
