(function () {

    var loaded = {};
    for (var i = 0; i < bf.widgetsrc.length; i++) {
        
        var scriptTag = document.createElement('script');
        scriptTag.setAttribute("type", "text/javascript");
        scriptTag.setAttribute("src", bf.widgetsrc);
        scriptTag.onload = function () { scriptLoadHandler(bf.widgetsrc[i]); };
        loaded[bf.widgetsrc] = false;
        (document.getElementsByTagName("head")[0] || document.documentElement).appendChild(scriptTag);
    }
    
    function scriptLoadHandler(src) {

        loaded[src] = true;
        for (var i = 0; i < bf.widgetsrc.length; i++) {
            if (!loaded[bf.widgetsrc[i]])
                return;
        }

        // Restore $ and window.jQuery to their previous values 
        var jQueryInt = window.jQuery.noConflict(true);

        bf.widgetload(window, jQueryInt);
    }

    for (var i = 0; i < bf.widgetcss.length; i++) {
        var cssTag = document.createElement('link');
        cssTag.setAttribute('rel', 'stylesheet');
        cssTag.setAttribute('type', 'text/css');
        cssTag.setAttribute('href', bf.widgetcss[i]);
        (document.getElementsByTagName("head")[0] || document.documentElement).appendChild(cssTag);
    }
    

})(); 