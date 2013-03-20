$(function () {


//    bf.page = new bf.ProfileViewModel(
//        new bf.SelectedFlierViewModel(new bf.BehaviourViewModelFactory(), location.pathname)
//        , new bf.TileLayoutViewModel('#created-fliers', new bf.BulletinLayoutProperties())
//        , new bf.TileLayoutViewModel('#claimed-fliers', new bf.BulletinLayoutProperties()));

    bf.pageinit[$('.page-content[data-pageid]').attr('data-pageid')]();
    bf.ScrollToTop();

});