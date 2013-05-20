
(function (window, $, undefined) {

    var bf = window.bf = window.bf || {};
    bf.pageinit = bf.pageinit || {};
    bf.pageinit['default'] = function () {
        var self = this;
        self.CreateFlierInstance = bf.globalCreateFlierInstance;
        ko.applyBindings(self);
    };
    
    bf.pagedefaultaction = bf.pagedefaultaction || {};
    bf.pagedefaultaction.set = function(page, action) {
        $.cookie(page + "action", action);
    };
    bf.pagedefaultaction.get = function (page) {
        var act = $.cookie(page + "action");
        $.removeCookie(page + "action");
        return act;
    };


    $(function () {
        var init = bf.pageinit[$('body[data-pageid]').attr('data-pageid')];
        if (init === undefined)
            init = bf.pageinit['default'];
        init();

        bf.ScrollToTop();
    });
    
})(window, JQuery);

