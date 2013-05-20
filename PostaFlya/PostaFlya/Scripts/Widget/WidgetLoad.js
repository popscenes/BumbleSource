(function () {

    var scriptTag = document.createElement('script');
    scriptTag.setAttribute("type", "text/javascript");
    scriptTag.setAttribute("src", bf.widgetsrc);
    if (scriptTag.readyState) {
        scriptTag.onreadystatechange = function () { // For old versions of IE
            if (this.readyState == 'complete' || this.readyState == 'loaded') {
                scriptLoadHandler();
            }
        };
    } else { // Other browsers
        scriptTag.onload = scriptLoadHandler;
    }
    (document.getElementsByTagName("head")[0] || document.documentElement).appendChild(scriptTag);

    function scriptLoadHandler() {
        // Restore $ and window.jQuery to their previous values and store the
        // new jQuery in our local jQuery variable
        var jQueryInt = window.jQuery.noConflict(true);

        bf.widgetload(window, jQueryInt);
 
    }

})(); 