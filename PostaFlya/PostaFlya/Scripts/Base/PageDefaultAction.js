
(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};

    bf.pagedefaultaction = bf.pagedefaultaction || {};
    bf.pagedefaultaction.set = function (page, action) {
        $.cookie(page + "action", action);
        bf.pagedefaultaction[page + "action"] = action;
    };
    bf.pagedefaultaction.get = function (page) {

        var act = $.cookie(page + "action");
        if (!act && bf.pagedefaultaction[page + "action"]) {
            act = bf.pagedefaultaction[page + "action"];
        }
        
        $.removeCookie(page + "action");
        bf.pagedefaultaction[page + "action"] = null;
        return act;
    };


})(window, jQuery);

