(function(window, undefined) {

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
        FB.init({ appId: "171501919670169", status: true, cookie: true });
    };

    bf.postToFacebook = function(link, image, title, desc, caption, redirect) {

        //stoopid
        var image = image.replace("127.0.0.1", "localhost");

        var obj = {
            method: 'feed',
            redirect_uri: "http://postaflya.com",
            link: link,
            picture: image,
            name: 'Postaflya',
            caption: caption,
            description: desc
        };

        function callback(response) {
            
        }

        FB.ui(obj, callback);
    };
    
})(window);