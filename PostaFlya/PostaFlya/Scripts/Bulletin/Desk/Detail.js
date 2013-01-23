/* File Created: June 15, 2012 */
$(function () {
    bf.page = new bf.BehaviourViewModel(new bf.BehaviourViewModelFactory(), location.pathname);
    bf.locforview = new bf.LocationService(false);
    bf.locforview.Start(function (ls) {
        var reqdata = { id: location.pathname };
        $.extend(reqdata, ls.currentLocation);
        $.get("/TrackView/Loc", reqdata);
    });
});