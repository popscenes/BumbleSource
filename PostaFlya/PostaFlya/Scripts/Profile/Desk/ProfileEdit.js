$(function () {

    bf.page = new bf.ProfileEditViewModel(
        new bf.LocationSelector({
            displayInline: false,
            mapElementId: 'map-profile',
            locSearchId: 'locationSearch-profile'
        }),
        
        new bf.ImageSelector());

});