(function(window, $, undefined) {

    var bf = window.bf = window.bf || {};

    (function(d, s, id, debug) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) {
            return;
        }
        js = d.createElement(s);
        js.id = id;
        js.src = "http://connect.facebook.net/en_US/all.js";
        fjs.parentNode.insertBefore(js, fjs);
    }(document, 'script', 'facebook-jssdk', /*debug*/ false));

    window.fbAsyncInit = function() {
        var appId = $("#fb-info").attr("data-app-id");
        FB.init({ appId: appId, status: true, cookie: true });
    };

    bf.postToFacebook = function(link, image, title, desc, caption) {

        //stoopid
        var image = image.replace("127.0.0.1", "localhost");
        
        var redirect = $("#fb-info").attr("data-redirect-post");

        var obj = {
            method: 'feed',
            redirect_uri: redirect,
            link: link,
            picture: image,
            name: 'Popscenes',
            caption: caption,
            description: desc
        };

        function callback(response) {
            
        }

        FB.ui(obj, callback);
    };
    
})(window, JQuery);