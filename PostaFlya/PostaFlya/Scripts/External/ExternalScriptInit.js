$(function () {
    //add this
//    (function (d, s, id) {
//        var js, fjs = d.getElementsByTagName(s)[0];
//        if (d.getElementById(id)) return;
//        js = d.createElement(s); js.id = id;
//        js.src = "//s7.addthis.com/js/250/addthis_widget.js#pubid=ra-4fa0804c3f4b7cc3";
//        fjs.parentNode.insertBefore(js, fjs);
//    } (document, 'script', 'addthissdk'));

//    (function (d, s, id) {
//        var js, fjs = d.getElementsByTagName(s)[0];
//        if (d.getElementById(id)) return;
//        js = d.createElement(s); js.id = id;
//        js.src = "//maps.google.com/maps/api/js?sensor=true";
//        fjs.parentNode.insertBefore(js, fjs);
    //    } (document, 'script', 'googlemaps'));

    (function (d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) return;
        js = d.createElement(s); js.id = id;
        js.src = "//connect.facebook.net/en_US/all.js#xfbml=1&appId=306027489468762";
        fjs.parentNode.insertBefore(js, fjs);
    } (document, 'script', 'facebook-jssdk'));

    (function (d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0]; 
        if (!d.getElementById(id)) { js = d.createElement(s); js.id = id; js.src = "//platform.twitter.com/widgets.js"; fjs.parentNode.insertBefore(js, fjs); }
    } (document, "script", "twitter-wjs"));

});