/* File Created: June 15, 2012 */
$(function () {
    bf.page = new bf.BehaviourViewModel(new bf.BehaviourViewModelFactory(), location.pathname);
    bf.locforview = new bf.LocationService(false);
    bf.locforview.StartTrackingLocation(function() {
        $.get("/TrackView/Loc", { loc: bf.locforview.currentLocation, id: location.pathname });
    });
});