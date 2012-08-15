$(function () {

    bf.page = new bf.ProfileViewModel(
        new bf.SelectedFlierViewModel(new bf.BehaviourViewModelFactory(), location.pathname)
        , new bf.TileLayoutViewModel('#created-fliers', new bf.BulletinLayoutProperties())
        , new bf.TileLayoutViewModel('#liked-fliers', new bf.BulletinLayoutProperties()));

    bf.ScrollToTop();

});