$(function () {

    bf.page = new bf.MyFliers(new bf.LocationSelector({
        displayInline: true
    })
    , new bf.ImageSelector()
    , new bf.TagsSelector()
    , new bf.NoLayoutViewModel(new bf.BulletinLayoutProperties()));

});