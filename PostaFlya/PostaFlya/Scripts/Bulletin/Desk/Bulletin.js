$(function () {
    
    bf.page = new bf.BulletinBoard(
        new bf.LocationSelector()
        , new bf.DistanceSelector()
        , new bf.SelectedFlierViewModel(new bf.BehaviourViewModelFactory())
        , new bf.TagsSelector()
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

    bf.ScrollToTop();

});
//